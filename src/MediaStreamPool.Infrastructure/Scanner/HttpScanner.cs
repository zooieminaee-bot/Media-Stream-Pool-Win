using System.Net.Http.Headers;
using System.Text;
using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Infrastructure.Scanner;

public sealed class HttpScanner(HttpClient httpClient, IDecoder decoder) : IScanner
{
    private const int MaxPayloadBytes = 10 * 1024 * 1024;

    public async Task<ScanResult> ScanAsync(Uri source, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            progress?.Report(new("Fetching", 0.10, source.AbsoluteUri));

            using var request = new HttpRequestMessage(HttpMethod.Get, source);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            progress?.Report(new("Reading payload", 0.30, $"HTTP {(int)response.StatusCode}"));
            var payload = await ReadLimitedAsync(response, cancellationToken);
            var text = Encoding.UTF8.GetString(payload);

            progress?.Report(new("Analyzing payload", 0.50));
            var texts = new List<(string Text, bool Decoded)> { (text, false) };
            var decode = await decoder.DecodeAsync(payload, cancellationToken);
            if (decode.Success && !string.IsNullOrWhiteSpace(decode.Content))
                texts.Add((decode.Content!, true));

            var records = new Dictionary<string, Record>(StringComparer.OrdinalIgnoreCase);
            var decodedPayloads = new List<DecodedPayload>();

            foreach (var item in texts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.Decoded)
                {
                    decodedPayloads.Add(new DecodedPayload
                    {
                        Id = Guid.NewGuid(),
                        OriginalPayload = text,
                        DecodedContent = item.Text,
                        ContentType = decode.ContentType,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }

                foreach (var uri in UrlExtractor.Extract(item.Text))
                {
                    var kind = RecordClassifier.Classify(uri, response.Content.Headers.ContentType?.MediaType);
                    records.TryAdd(uri.AbsoluteUri, new Record
                    {
                        Id = Guid.NewGuid(),
                        Kind = kind,
                        Url = uri.AbsoluteUri,
                        Source = source.AbsoluteUri,
                        Status = response.IsSuccessStatusCode ? "OK" : "HTTP_ERROR",
                        StatusCode = (int)response.StatusCode,
                        ContentType = response.Content.Headers.ContentType?.MediaType,
                        Method = HttpMethod.Get.Method,
                        Format = RecordClassifier.InferFormat(uri, response.Content.Headers.ContentType?.MediaType),
                        Host = uri.Host,
                        Path = uri.AbsolutePath,
                        Payload = item.Decoded ? null : text,
                        Decoded = item.Decoded ? item.Text : null,
                        DiscoveredAt = DateTimeOffset.UtcNow
                    });
                }
            }

            progress?.Report(new("Completed", 1.0, $"{records.Count} unique records"));
            return new ScanResult(response.IsSuccessStatusCode, records.Values.ToArray(), decodedPayloads);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return new ScanResult(false, [], [], ex.Message);
        }
        catch (Exception ex)
        {
            return new ScanResult(false, [], [], ex.Message);
        }
    }

    private static async Task<byte[]> ReadLimitedAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength is > MaxPayloadBytes)
            throw new InvalidOperationException($"Payload exceeds the {MaxPayloadBytes / (1024 * 1024)} MB scanner limit.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        var total = 0;
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            total += read;
            if (total > MaxPayloadBytes)
                throw new InvalidOperationException($"Payload exceeds the {MaxPayloadBytes / (1024 * 1024)} MB scanner limit.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
        return buffer.ToArray();
    }
}
