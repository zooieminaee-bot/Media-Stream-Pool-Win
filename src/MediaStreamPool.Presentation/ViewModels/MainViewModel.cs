using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MediaStreamPool.Domain.Interfaces;
using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Presentation.ViewModels;

public partial class MainViewModel(
    IRecordRepository records,
    IDecodedPayloadRepository decodedPayloads,
    IScanRunRepository scanRuns) : ObservableObject
{
    public ObservableCollection<ScanRun> RecentScans { get; } = [];

    [ObservableProperty] private int totalRecords;
    [ObservableProperty] private int streams;
    [ObservableProperty] private int apis;
    [ObservableProperty] private int decodedPayloads;
    [ObservableProperty] private string status = "Ready";

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        Status = "Refreshing local index...";
        TotalRecords = await records.CountAsync(cancellationToken: cancellationToken);
        Streams = await records.CountAsync(RecordKind.Stream, cancellationToken);
        Apis = await records.CountAsync(RecordKind.Api, cancellationToken);
        DecodedPayloads = await decodedPayloads.CountAsync(cancellationToken);

        var recent = await scanRuns.GetRecentAsync(8, cancellationToken);
        RecentScans.Clear();
        foreach (var scan in recent)
            RecentScans.Add(scan);

        Status = "Ready";
    }
}
