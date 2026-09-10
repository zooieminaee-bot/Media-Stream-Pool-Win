using MediaStreamPool.Domain.Models;
using MediaStreamPool.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace MediaStreamPool.App.Pages;

public sealed partial class RecordsPage : Page
{
    private readonly RecordsViewModel _viewModel;
    private Record? _selected;

    public RecordsPage()
    {
        InitializeComponent();
        _viewModel = App.Services.GetRequiredService<RecordsViewModel>();
        DataContext = _viewModel;
    }

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            var kind = e.Parameter?.ToString() == "Apis" ? RecordKind.Api : RecordKind.Stream;
            TitleText.Text = kind == RecordKind.Api ? "APIs" : "Streams";
            await _viewModel.LoadAsync(kind);
            RecordsList.ItemsSource = _viewModel.Items;
        }
        catch (Exception ex)
        {
            _viewModel.Status = $"Load failed: {ex.Message}";
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.SearchText = SearchBox.Text;
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            _viewModel.Status = $"Refresh failed: {ex.Message}";
        }
    }

    private void RecordsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = RecordsList.SelectedItem as Record;
        if (_selected is null)
        {
            EmptyDetails.Visibility = Visibility.Visible;
            RecordDetails.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyDetails.Visibility = Visibility.Collapsed;
        RecordDetails.Visibility = Visibility.Visible;
        DetailUrl.Text = _selected.Url;
        DetailSource.Text = $"Source: {_selected.Source ?? "Unknown"}";
        DetailStatus.Text = $"Status: {_selected.Status ?? "Unknown"} ({_selected.StatusCode?.ToString() ?? "-"})";
        DetailContentType.Text = $"Content-Type: {_selected.ContentType ?? "Unknown"}";
        DetailPath.Text = $"Path: {_selected.Path ?? "-"}";
    }

    private void CopyUrl_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null) return;
        var package = new DataPackage();
        package.SetText(_selected.Url);
        Clipboard.SetContent(package);
    }
}
