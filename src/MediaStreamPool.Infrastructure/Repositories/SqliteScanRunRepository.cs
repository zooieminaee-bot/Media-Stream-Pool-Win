using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;
using Microsoft.Data.Sqlite;

namespace MediaStreamPool.Infrastructure.Repositories;

public sealed class SqliteScanRunRepository(string connectionString) : IScanRunRepository
{
    public async Task AddAsync(ScanRun scanRun, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ScanRuns (Id, SourceUrl, Status, RecordsFound, Error, StartedAt, CompletedAt)
            VALUES ($id, $sourceUrl, $status, $recordsFound, $error, $startedAt, $completedAt);
            """;
        command.Parameters.AddWithValue("$id", scanRun.Id.ToString());
        command.Parameters.AddWithValue("$sourceUrl", scanRun.SourceUrl);
        command.Parameters.AddWithValue("$status", (int)scanRun.Status);
        command.Parameters.AddWithValue("$recordsFound", scanRun.RecordsFound);
        command.Parameters.AddWithValue("$error", (object?)scanRun.Error ?? DBNull.Value);
        command.Parameters.AddWithValue("$startedAt", scanRun.StartedAt.ToString("O"));
        command.Parameters.AddWithValue("$completedAt", scanRun.CompletedAt?.ToString("O") ?? (object)DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScanRun>> GetRecentAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SourceUrl, Status, RecordsFound, Error, StartedAt, CompletedAt FROM ScanRuns ORDER BY StartedAt DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", limit);

        var result = new List<ScanRun>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ScanRun
            {
                Id = Guid.Parse(reader.GetString(0)), SourceUrl = reader.GetString(1), Status = (ScanRunStatus)reader.GetInt32(2),
                RecordsFound = reader.GetInt32(3), Error = reader.IsDBNull(4) ? null : reader.GetString(4),
                StartedAt = DateTimeOffset.Parse(reader.GetString(5)), CompletedAt = reader.IsDBNull(6) ? null : DateTimeOffset.Parse(reader.GetString(6))
            });
        }
        return result;
    }
}

public sealed class SqliteDecodedPayloadRepository(string connectionString) : IDecodedPayloadRepository
{
    public async Task AddAsync(DecodedPayload payload, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO DecodedPayloads (Id, ScanRunId, OriginalPayload, DecodedContent, ContentType, CreatedAt)
            VALUES ($id, $scanRunId, $original, $decoded, $contentType, $createdAt);
            """;
        command.Parameters.AddWithValue("$id", payload.Id.ToString());
        command.Parameters.AddWithValue("$scanRunId", payload.ScanRunId?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$original", payload.OriginalPayload);
        command.Parameters.AddWithValue("$decoded", payload.DecodedContent);
        command.Parameters.AddWithValue("$contentType", (object?)payload.ContentType ?? DBNull.Value);
        command.Parameters.AddWithValue("$createdAt", payload.CreatedAt.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DecodedPayload>> SearchAsync(string? query = null, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, ScanRunId, OriginalPayload, DecodedContent, ContentType, CreatedAt
            FROM DecodedPayloads
            WHERE $query = ''
               OR OriginalPayload LIKE $pattern
               OR DecodedContent LIKE $pattern
               OR COALESCE(ContentType, '') LIKE $pattern
            ORDER BY CreatedAt DESC;
            """;
        var normalized = query?.Trim() ?? string.Empty;
        command.Parameters.AddWithValue("$query", normalized);
        command.Parameters.AddWithValue("$pattern", $"%{normalized}%");

        var result = new List<DecodedPayload>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new DecodedPayload
            {
                Id = Guid.Parse(reader.GetString(0)),
                ScanRunId = reader.IsDBNull(1) ? null : Guid.Parse(reader.GetString(1)),
                OriginalPayload = reader.GetString(2),
                DecodedContent = reader.GetString(3),
                ContentType = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedAt = DateTimeOffset.Parse(reader.GetString(5))
            });
        }
        return result;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM DecodedPayloads;";
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }
}
