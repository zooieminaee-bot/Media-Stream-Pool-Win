using CommunityToolkit.Mvvm.ComponentModel;
using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Presentation.ViewModels;

public partial class MainViewModel(IRecordRepository records) : ObservableObject
{
    [ObservableProperty] private int totalRecords;
    [ObservableProperty] private int streams;
    [ObservableProperty] private int apis;
    [ObservableProperty] private string status = "Ready";

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        TotalRecords = await records.CountAsync(cancellationToken: cancellationToken);
        Streams = await records.CountAsync(RecordKind.Stream, cancellationToken);
        Apis = await records.CountAsync(RecordKind.Api, cancellationToken);
        Status = "Ready";
    }
}
