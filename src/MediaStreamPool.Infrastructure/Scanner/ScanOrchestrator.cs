using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Infrastructure.Scanner;

public sealed class ScanOrchestrator(
    IScanner scanner,
    IRecordRepository records,
    IScanRunRepository scanRuns,
    IDecodedPayloadRepository decodedPayloads) : IScanOrchestrator
{
    public async Task<ScanResult> RunAsync(Uri source, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var runId = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;

        progress?.Report(new("Starting", 0.0, source.AbsoluteUri));

        try
        {
            var result = await scanner.ScanAsync(source, progress, cancellationToken);

            foreach (var record in result.Records)
                await records.AddAsync(record, cancellationToken);

            foreach (var payload in result.DecodedPayloads)
                await decodedPayloads.AddAsync(payload with { ScanRunId = runId }, cancellationToken);

            await scanRuns.AddAsync(new ScanRun
            {
                Id = runId,
                SourceUrl = source.AbsoluteUri,
                Status = result.Success ? ScanRunStatus.Completed : ScanRunStatus.Failed,
                RecordsFound = result.Records.Count,
                Error = result.Error,
                StartedAt = startedAt,
                CompletedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
        {
            await scanRuns.AddAsync(new ScanRun
            {
                Id = runId,
                SourceUrl = source.AbsoluteUri,
                Status = ScanRunStatus.Cancelled,
                StartedAt = startedAt,
                CompletedAt = DateTimeOffset.UtcNow
            });
            throw;
        }
        catch (Exception ex)
        {
            await scanRuns.AddAsync(new ScanRun
            {
                Id = runId,
                SourceUrl = source.AbsoluteUri,
                Status = ScanRunStatus.Failed,
                Error = ex.Message,
                StartedAt = startedAt,
                CompletedAt = DateTimeOffset.UtcNow
            });
            throw;
        }
    }
}
