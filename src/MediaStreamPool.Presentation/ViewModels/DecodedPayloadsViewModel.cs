using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Presentation.ViewModels;

public partial class DecodedPayloadsViewModel(IDecodedPayloadRepository repository) : ObservableObject
{
    public ObservableCollection<DecodedPayload> Items { get; } = [];

    [ObservableProperty] private DecodedPayload? selectedPayload;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string status = "Ready";
    [ObservableProperty] private string formattedContent = string.Empty;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Status = "Loading...";
        var items = await repository.SearchAsync(
            string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            cancellationToken);

        Items.Clear();
        foreach (var item in items)
            Items.Add(item);

        Status = $"{Items.Count:N0} payload(s)";
    }

    partial void OnSelectedPayloadChanged(DecodedPayload? value)
    {
        FormattedContent = value is null ? string.Empty : FormatContent(value.DecodedContent, value.ContentType);
    }

    private static string FormatContent(string content, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType) && contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                // Preserve the original content when it is not valid JSON.
            }
        }

        return content;
    }
}
