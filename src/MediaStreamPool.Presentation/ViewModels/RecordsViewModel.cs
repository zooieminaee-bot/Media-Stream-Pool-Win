using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Presentation.ViewModels;

public partial class RecordsViewModel(IRecordRepository repository) : ObservableObject
{
    public ObservableCollection<Record> Items { get; } = [];

    [ObservableProperty] private Record? selectedRecord;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string status = "Ready";

    public RecordKind Kind { get; private set; }

    public async Task LoadAsync(RecordKind kind, CancellationToken cancellationToken = default)
    {
        Kind = kind;
        await RefreshAsync(cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Status = "Loading...";
        var records = await repository.SearchAsync(
            string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim(),
            Kind,
            cancellationToken);

        Items.Clear();
        foreach (var record in records)
            Items.Add(record);

        Status = $"{Items.Count:N0} record(s)";
    }

    partial void OnSearchTextChanged(string value) => _ = RefreshAsync();
}
