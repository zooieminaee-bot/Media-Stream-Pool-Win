using MediaStreamPool.Domain.Models;
using MediaStreamPool.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace MediaStreamPool.App.Pages;

public sealed partial class DecodedPayloadsPage : Page
{
    private readonly DecodedPayloadsViewModel _viewModel;
    private DecodedPayload? _selected;

    public DecodedPayloadsPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<DecodedPayloadsViewModel>();
        DataContext = _viewModel;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            await _viewModel.RefreshAsync();
            PayloadList.ItemsSource = _viewModel.Items;
        }
        catch (Exception ex)
        {
            ShowStatus($"Could not load decoded payloads: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.SearchText = SearchBox.Text;
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Search failed: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            ShowStatus($"Refresh failed: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private void PayloadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = PayloadList.SelectedItem as DecodedPayload;
        ContentTextBox.Text = _viewModel.FormattedContent;
        MetadataText.Text = _selected is null
            ? string.Empty
            : $"Content-Type: {_selected.ContentType ?? "Unknown"}  |  Created: {_selected.CreatedAt:yyyy-MM-dd HH:mm:ss zzz}  |  Scan: {_selected.ScanRunId?.ToString() ?? "-"}";
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var package = new DataPackage();
        package.SetText(_viewModel.FormattedContent);
        Clipboard.SetContent(package);
        ShowStatus("Content copied to clipboard.", InfoBarSeverity.Success);
    }

    private void ShowStatus(string message, InfoBarSeverity severity)
    {
        StatusBar.Title = severity == InfoBarSeverity.Error ? "Error" : "Decoded Payloads";
        StatusBar.Message = message;
        StatusBar.Severity = severity;
        StatusBar.IsOpen = true;
    }
}
