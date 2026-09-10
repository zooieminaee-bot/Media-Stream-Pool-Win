using Microsoft.UI.Xaml.Controls;
using MediaStreamPool.Presentation.ViewModels;

namespace MediaStreamPool.App.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.Parameter is MainViewModel vm)
            DataContext = vm;
        base.OnNavigatedTo(e);
    }
}
