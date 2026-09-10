namespace MediaStreamPool.Domain.Models;

public sealed record ScanRun
{
    public Guid Id { get; init; }
    public required string SourceUrl { get; init; }
    public ScanRunStatus Status { get; init; }
    public int RecordsFound { get; init; }
    public string? Error { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}

public enum ScanRunStatus
{
    Running,
    Completed,
    Failed,
    Cancelled
}

public sealed record Source
{
    public Guid Id { get; init; }
    public required string Url { get; init; }
    public string? Name { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record DecodedPayload
{
    public Guid Id { get; init; }
    public Guid? ScanRunId { get; init; }
    public required string OriginalPayload { get; init; }
    public required string DecodedContent { get; init; }
    public string? ContentType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
