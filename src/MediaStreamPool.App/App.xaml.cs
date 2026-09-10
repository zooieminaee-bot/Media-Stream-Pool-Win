using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
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
        var window = Services.GetRequiredService<MainWindow>();
        window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        services.AddSingleton<MainWindow>();
        services.AddTransient<MainViewModel>();
        return services.BuildServiceProvider();
    }
}
