using MediaStreamPool.Infrastructure.Scanner;
using Xunit;

namespace MediaStreamPool.Tests;

public sealed class UrlExtractorTests
{
    [Fact]
    public void ExtractsAndDeduplicatesHttpAndRtmpUrls()
    {
        const string payload = "https://example.com/a.m3u8, https://example.com/a.m3u8 rtmp://media.example/live/one";

        var urls = UrlExtractor.Extract(payload);

        Assert.Equal(2, urls.Count);
        Assert.Contains(urls, u => u.AbsoluteUri == "https://example.com/a.m3u8");
        Assert.Contains(urls, u => u.Scheme == "rtmp");
    }
}
