using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MediaStreamPool.App.Pages;

public sealed partial class PlaceholderPage : Page
{
    public PlaceholderPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string title)
            TitleText.Text = title;
        base.OnNavigatedTo(e);
    }
}
