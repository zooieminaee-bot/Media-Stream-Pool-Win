using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using MediaStreamPool.Domain.Interfaces;

namespace MediaStreamPool.Infrastructure.Decoder;

public sealed class AesGzipDecoder : IDecoder
{
    private readonly byte[]? _key;
    private readonly byte[]? _iv;

    public AesGzipDecoder(string? keyHex, string? ivHex)
    {
        _key = ParseHex(keyHex, 32);
        _iv = ParseHex(ivHex, 16);
    }

    public async Task<DecodeResult> DecodeAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default)
    {
        if (_key is null || _iv is null)
            return DecodeResult.Failed("Decoder key/IV are not configured.");

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encrypted = Convert.FromBase64String(Encoding.UTF8.GetString(payload.Span).Trim());

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            await using var decrypted = new MemoryStream();
            await using (var crypto = new CryptoStream(decrypted, aes.CreateDecryptor(), CryptoStreamMode.Write, leaveOpen: true))
            {
                await crypto.WriteAsync(encrypted, cancellationToken);
                await crypto.FlushFinalBlockAsync(cancellationToken);
            }

            decrypted.Position = 0;
            await using var gzip = new GZipStream(decrypted, CompressionMode.Decompress, leaveOpen: false);
            using var reader = new StreamReader(gzip, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var text = await reader.ReadToEndAsync(cancellationToken);

            var contentType = LooksLikeJson(text) ? "application/json" : "text/plain";
            return DecodeResult.Succeeded(text, contentType);
        }
        catch (FormatException ex)
        {
            return DecodeResult.Failed($"Invalid Base64 payload: {ex.Message}");
        }
        catch (CryptographicException ex)
        {
            return DecodeResult.Failed($"AES decryption failed: {ex.Message}");
        }
        catch (InvalidDataException ex)
        {
            return DecodeResult.Failed($"GZIP decompression failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DecodeResult.Failed($"Decode failed: {ex.Message}");
        }
    }

    private static byte[]? ParseHex(string? value, int expectedBytes)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (value.Length != expectedBytes * 2)
            throw new ArgumentException($"Expected {expectedBytes * 2} hexadecimal characters.");
        return Convert.FromHexString(value);
    }

    private static bool LooksLikeJson(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }
}
