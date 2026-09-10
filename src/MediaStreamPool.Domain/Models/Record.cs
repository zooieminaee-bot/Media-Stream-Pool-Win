namespace MediaStreamPool.Domain.Models;

public sealed record Record
{
    public Guid Id { get; init; }
    public RecordKind Kind { get; init; }
    public required string Url { get; init; }
    public string? Source { get; init; }
    public string? Status { get; init; }
    public int? StatusCode { get; init; }
    public string? ContentType { get; init; }
    public string? Method { get; init; }
    public string? Format { get; init; }
    public string? Host { get; init; }
    public string? Path { get; init; }
    public string? Payload { get; init; }
    public string? Decoded { get; init; }
    public DateTimeOffset DiscoveredAt { get; init; }
}

public enum RecordKind
{
    Stream,
    Api,
    DecodedPayload
}
