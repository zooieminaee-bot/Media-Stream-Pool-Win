using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace MediaStreamPool.App.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DatabasePathText.Text = Path.Combine(local, "MediaStreamPool", "pool.db");
        DiagnosticsPathText.Text = Path.Combine(local, "MediaStreamPool", "startup.log");
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeComboBox.SelectedIndex < 0)
            return;

        RequestedTheme = ThemeComboBox.SelectedIndex switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }
}
