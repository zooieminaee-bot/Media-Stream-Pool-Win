using Microsoft.Data.Sqlite;

namespace MediaStreamPool.Infrastructure.Database;

public sealed class DatabaseInitializer(string connectionString)
{
    private const int InitialSchemaVersion = 1;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = (SqliteTransaction)transaction;
        command.CommandText = """
            PRAGMA foreign_keys = ON;
            PRAGMA journal_mode = WAL;

            CREATE TABLE IF NOT EXISTS SchemaMigrations (
                Version INTEGER NOT NULL PRIMARY KEY
            );

            CREATE TABLE IF NOT EXISTS Sources (
                Id TEXT NOT NULL PRIMARY KEY,
                Url TEXT NOT NULL UNIQUE,
                Name TEXT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ScanRuns (
                Id TEXT NOT NULL PRIMARY KEY,
                SourceUrl TEXT NOT NULL,
                Status INTEGER NOT NULL,
                RecordsFound INTEGER NOT NULL DEFAULT 0,
                Error TEXT NULL,
                StartedAt TEXT NOT NULL,
                CompletedAt TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS DecodedPayloads (
                Id TEXT NOT NULL PRIMARY KEY,
                ScanRunId TEXT NULL,
                OriginalPayload TEXT NOT NULL,
                DecodedContent TEXT NOT NULL,
                ContentType TEXT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (ScanRunId) REFERENCES ScanRuns(Id) ON DELETE SET NULL
            );

            CREATE TABLE IF NOT EXISTS Records (
                Id TEXT NOT NULL PRIMARY KEY,
                Kind INTEGER NOT NULL,
                Url TEXT NOT NULL UNIQUE,
                Source TEXT NULL,
                Status TEXT NULL,
                StatusCode INTEGER NULL,
                ContentType TEXT NULL,
                Method TEXT NULL,
                Format TEXT NULL,
                Host TEXT NULL,
                Path TEXT NULL,
                Payload TEXT NULL,
                Decoded TEXT NULL,
                DiscoveredAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT NOT NULL PRIMARY KEY,
                Value TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Records_Kind ON Records(Kind);
            CREATE INDEX IF NOT EXISTS IX_Records_Host ON Records(Host);
            CREATE INDEX IF NOT EXISTS IX_Records_DiscoveredAt ON Records(DiscoveredAt DESC);
            CREATE INDEX IF NOT EXISTS IX_ScanRuns_StartedAt ON ScanRuns(StartedAt DESC);
            CREATE INDEX IF NOT EXISTS IX_DecodedPayloads_CreatedAt ON DecodedPayloads(CreatedAt DESC);

            INSERT OR IGNORE INTO SchemaMigrations (Version) VALUES (1);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
