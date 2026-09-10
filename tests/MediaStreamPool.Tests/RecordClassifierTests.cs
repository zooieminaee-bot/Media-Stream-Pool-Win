using MediaStreamPool.Domain.Models;
using MediaStreamPool.Infrastructure.Scanner;

namespace MediaStreamPool.Tests;

public sealed class RecordClassifierTests
{
    [Theory]
    [InlineData("https://example.com/live/index.m3u8", "HLS", RecordKind.Stream)]
    [InlineData("https://example.com/video/manifest.mpd", "DASH", RecordKind.Stream)]
    [InlineData("rtmp://example.com/live/channel", "RTMP", RecordKind.Stream)]
    [InlineData("https://example.com/api/items", null, RecordKind.Api)]
    public void ClassifiesKnownEndpointTypes(string url, string? format, RecordKind expectedKind)
    {
        var uri = new Uri(url);

        Assert.Equal(expectedKind, RecordClassifier.Classify(uri));
        Assert.Equal(format, RecordClassifier.InferFormat(uri));
    }
}
