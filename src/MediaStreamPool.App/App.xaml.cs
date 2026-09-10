using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MediaStreamPool.Infrastructure.Database;
using MediaStreamPool.Infrastructure.DependencyInjection;
using MediaStreamPool.Presentation.ViewModels;

namespace MediaStreamPool.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Services = ConfigureServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _ = LaunchAsync();
    }

    private async Task LaunchAsync()
    {
        var window = Services.GetRequiredService<MainWindow>();
        var database = Services.GetRequiredService<DatabaseInitializer>();
        await window.InitializeAsync(database);
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<RecordsViewModel>();
        return services.BuildServiceProvider();
    }
}
