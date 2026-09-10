using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;
using Microsoft.Data.Sqlite;

namespace MediaStreamPool.Infrastructure.Repositories;

public sealed class SqliteRecordRepository(string connectionString) : IRecordRepository
{
    public async Task AddAsync(Record record, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Records
            (Id, Kind, Url, Source, Status, StatusCode, ContentType, Method, Format, Host, Path, Payload, Decoded, DiscoveredAt)
            VALUES ($id, $kind, $url, $source, $status, $statusCode, $contentType, $method, $format, $host, $path, $payload, $decoded, $discoveredAt)
            ON CONFLICT(Url) DO UPDATE SET
                Status = excluded.Status,
                StatusCode = excluded.StatusCode,
                ContentType = excluded.ContentType,
                DiscoveredAt = excluded.DiscoveredAt;
            """;
        AddParameters(command, record);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Record>> SearchAsync(string? query = null, RecordKind? kind = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Kind, Url, Source, Status, StatusCode, ContentType, Method, Format, Host, Path, Payload, Decoded, DiscoveredAt
            FROM Records
            WHERE ($query IS NULL OR Url LIKE $pattern OR Host LIKE $pattern OR Path LIKE $pattern OR Source LIKE $pattern)
              AND ($kind IS NULL OR Kind = $kind)
            ORDER BY DiscoveredAt DESC;
            """;
        command.Parameters.AddWithValue("$query", (object?)query ?? DBNull.Value);
        command.Parameters.AddWithValue("$pattern", query is null ? DBNull.Value : $"%{query}%");
        command.Parameters.AddWithValue("$kind", kind is null ? DBNull.Value : (object)(int)kind.Value);

        var result = new List<Record>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Add(Map(reader));
        return result;
    }

    public async Task<int> CountAsync(RecordKind? kind = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Records WHERE ($kind IS NULL OR Kind = $kind);";
        command.Parameters.AddWithValue("$kind", kind is null ? DBNull.Value : (object)(int)kind.Value);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static void AddParameters(SqliteCommand command, Record record)
    {
        command.Parameters.AddWithValue("$id", record.Id.ToString());
        command.Parameters.AddWithValue("$kind", (int)record.Kind);
        command.Parameters.AddWithValue("$url", record.Url);
        command.Parameters.AddWithValue("$source", (object?)record.Source ?? DBNull.Value);
        command.Parameters.AddWithValue("$status", (object?)record.Status ?? DBNull.Value);
        command.Parameters.AddWithValue("$statusCode", (object?)record.StatusCode ?? DBNull.Value);
        command.Parameters.AddWithValue("$contentType", (object?)record.ContentType ?? DBNull.Value);
        command.Parameters.AddWithValue("$method", (object?)record.Method ?? DBNull.Value);
        command.Parameters.AddWithValue("$format", (object?)record.Format ?? DBNull.Value);
        command.Parameters.AddWithValue("$host", (object?)record.Host ?? DBNull.Value);
        command.Parameters.AddWithValue("$path", (object?)record.Path ?? DBNull.Value);
        command.Parameters.AddWithValue("$payload", (object?)record.Payload ?? DBNull.Value);
        command.Parameters.AddWithValue("$decoded", (object?)record.Decoded ?? DBNull.Value);
        command.Parameters.AddWithValue("$discoveredAt", record.DiscoveredAt.ToString("O"));
    }

    private static Record Map(SqliteDataReader r) => new()
    {
        Id = Guid.Parse(r.GetString(0)), Kind = (RecordKind)r.GetInt32(1), Url = r.GetString(2),
        Source = r.IsDBNull(3) ? null : r.GetString(3), Status = r.IsDBNull(4) ? null : r.GetString(4),
        StatusCode = r.IsDBNull(5) ? null : r.GetInt32(5), ContentType = r.IsDBNull(6) ? null : r.GetString(6),
        Method = r.IsDBNull(7) ? null : r.GetString(7), Format = r.IsDBNull(8) ? null : r.GetString(8),
        Host = r.IsDBNull(9) ? null : r.GetString(9), Path = r.IsDBNull(10) ? null : r.GetString(10),
        Payload = r.IsDBNull(11) ? null : r.GetString(11), Decoded = r.IsDBNull(12) ? null : r.GetString(12),
        DiscoveredAt = DateTimeOffset.Parse(r.GetString(13))
    };
}
