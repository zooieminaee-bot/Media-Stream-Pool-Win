using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MediaStreamPool.Domain.Interfaces;

namespace MediaStreamPool.App.Pages;

public sealed partial class ScannerPage : Page
{
    private readonly IScanOrchestrator _scanner;
    private CancellationTokenSource? _scanCancellation;

    public ScannerPage()
    {
        InitializeComponent();
        _scanner = App.Services.GetRequiredService<IScanOrchestrator>();
    }

    private async void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Uri.TryCreate(SourceTextBox.Text.Trim(), UriKind.Absolute, out var source) ||
            source.Scheme is not ("http" or "https"))
        {
            ShowStatus("Invalid source URL. Use an HTTP or HTTPS URL you are authorized to access.", InfoBarSeverity.Error);
            return;
        }

        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
        _scanCancellation = new CancellationTokenSource();

        ScanButton.IsEnabled = false;
        Progress.Visibility = Visibility.Visible;
        Progress.Value = 0;
        StatusBar.IsOpen = false;
        ResultSummary.Text = string.Empty;

        var progress = new Progress<ScanProgress>(p =>
        {
            Progress.Value = p.Percentage;
            StatusBar.Title = p.Stage;
            StatusBar.Message = p.Message ?? string.Empty;
            StatusBar.IsOpen = true;
            StatusBar.Severity = InfoBarSeverity.Informational;
        });

        try
        {
            var result = await _scanner.RunAsync(source, progress, _scanCancellation.Token);
            ResultSummary.Text = result.Success
                ? $"Scan completed. Persisted {result.Records.Count} unique records and {result.DecodedPayloads.Count} decoded payload(s)."
                : $"Scan failed: {result.Error}";
            ShowStatus(result.Success ? "Scan completed" : "Scan failed", result.Success ? InfoBarSeverity.Success : InfoBarSeverity.Error);
        }
        catch (OperationCanceledException)
        {
            ShowStatus("Scan cancelled.", InfoBarSeverity.Warning);
        }
        catch (Exception ex)
        {
            ShowStatus($"Unexpected scanner error: {ex.Message}", InfoBarSeverity.Error);
        }
        finally
        {
            ScanButton.IsEnabled = true;
            Progress.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowStatus(string message, InfoBarSeverity severity)
    {
        StatusBar.Title = severity == InfoBarSeverity.Error ? "Error" : "Scanner";
        StatusBar.Message = message;
        StatusBar.Severity = severity;
        StatusBar.IsOpen = true;
    }
}
