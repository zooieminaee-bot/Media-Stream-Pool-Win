using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Infrastructure.Database;
using MediaStreamPool.Infrastructure.Decoder;
using MediaStreamPool.Infrastructure.Repositories;
using MediaStreamPool.Infrastructure.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace MediaStreamPool.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediaStreamPool");
        Directory.CreateDirectory(appData);
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(appData, "pool.db"),
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared
        }.ToString();

        services.AddSingleton(new DatabaseInitializer(connectionString));
        services.AddSingleton<IRecordRepository>(new SqliteRecordRepository(connectionString));
        services.AddHttpClient<IScanner, HttpScanner>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MediaStreamPool/0.1");
        });

        services.AddSingleton<IDecoder>(_ => new AesGzipDecoder(
            Environment.GetEnvironmentVariable("MEDIA_POOL_AES_KEY_HEX"),
            Environment.GetEnvironmentVariable("MEDIA_POOL_AES_IV_HEX")));

        return services;
    }
}
