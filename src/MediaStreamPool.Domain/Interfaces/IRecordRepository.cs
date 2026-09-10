using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Domain.Interfaces;

public interface IRecordRepository
{
    Task AddAsync(Record record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Record>> SearchAsync(string? query = null, RecordKind? kind = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(RecordKind? kind = null, CancellationToken cancellationToken = default);
}

public interface IScanRunRepository
{
    Task AddAsync(ScanRun scanRun, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScanRun>> GetRecentAsync(int limit = 20, CancellationToken cancellationToken = default);
}

public interface IDecodedPayloadRepository
{
    Task AddAsync(DecodedPayload payload, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DecodedPayload>> SearchAsync(string? query = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

public interface IDecoder
{
    Task<DecodeResult> DecodeAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}

public sealed record DecodeResult(bool Success, string? Content, string? Error, string? ContentType)
{
    public static DecodeResult Failed(string error) => new(false, null, error, null);
    public static DecodeResult Succeeded(string content, string? contentType = null) => new(true, content, null, contentType);
}

public interface IScanner
{
    Task<ScanResult> ScanAsync(Uri source, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default);
}

public interface IScanOrchestrator
{
    Task<ScanResult> RunAsync(Uri source, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default);
}

public sealed record ScanProgress(string Stage, double Percentage, string? Message = null);

public sealed record ScanResult(
    bool Success,
    IReadOnlyList<Record> Records,
    IReadOnlyList<DecodedPayload> DecodedPayloads,
    string? Error = null);
