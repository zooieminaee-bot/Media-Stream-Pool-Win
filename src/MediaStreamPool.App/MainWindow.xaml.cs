using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MediaStreamPool.Infrastructure.Database;
using MediaStreamPool.Presentation.ViewModels;
using MediaStreamPool.App.Pages;

namespace MediaStreamPool.App;

public sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        Title = "Media Stream Pool";
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage), _viewModel);
    }

    public async Task InitializeAsync(DatabaseInitializer database)
    {
        await database.InitializeAsync();
        await _viewModel.RefreshAsync();
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
            return;

        switch (tag)
        {
            case "Dashboard":
                ContentFrame.Navigate(typeof(DashboardPage), _viewModel);
                break;
            case "Streams":
            case "Apis":
                ContentFrame.Navigate(typeof(RecordsPage), tag);
                break;
            case "Decoded":
                ContentFrame.Navigate(typeof(DecodedPayloadsPage));
                break;
            case "Scanner":
                ContentFrame.Navigate(typeof(ScannerPage));
                break;
            case "Settings":
                ContentFrame.Navigate(typeof(SettingsPage));
                break;
            case "About":
                ContentFrame.Navigate(typeof(AboutPage));
                break;
        }
    }
}
