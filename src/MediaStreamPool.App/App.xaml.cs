using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MediaStreamPool.Infrastructure.Database;
using MediaStreamPool.Infrastructure.DependencyInjection;
using MediaStreamPool.Presentation.ViewModels;
using System.Diagnostics;

namespace MediaStreamPool.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        UnhandledException += OnUnhandledException;
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _ = LaunchAsync();
    }

    private async Task LaunchAsync()
    {
        try
        {
            Log("Application launch started.");

            var window = Services.GetRequiredService<MainWindow>();
            var database = Services.GetRequiredService<DatabaseInitializer>();

            await window.InitializeAsync(database);
            window.Activate();

            Log("Application launch completed.");
        }
        catch (Exception ex)
        {
            LogException("Application launch failed.", ex);
            ShowStartupError(ex);
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
    {
        LogException("Unhandled WinUI exception.", args.Exception);
        args.Handled = true;
    }

    private static void ShowStartupError(Exception exception)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Media Stream Pool could not start",
                Content = $"A startup error occurred. A diagnostic log was written to:\n\n{GetLogPath()}\n\n{exception.Message}",
                CloseButtonText = "Close"
            };

            _ = dialog.ShowAsync();
        }
        catch
        {
            // If WinUI itself cannot initialize, the file log remains the diagnostic source.
        }
    }

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetLogPath())!);
            File.AppendAllText(GetLogPath(), $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never prevent application startup.
        }
    }

    private static void LogException(string message, Exception exception)
    {
        Log($"{message}{Environment.NewLine}{exception}");
        Debug.WriteLine($"{message}{Environment.NewLine}{exception}");
    }

    private static string GetLogPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MediaStreamPool",
            "startup.log");
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<RecordsViewModel>();
        services.AddTransient<DecodedPayloadsViewModel>();
        return services.BuildServiceProvider();
    }
}
