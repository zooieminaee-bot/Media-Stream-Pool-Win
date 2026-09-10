using MediaStreamPool.Domain.Models;

namespace MediaStreamPool.Infrastructure.Scanner;

public static class RecordClassifier
{
    public static RecordKind Classify(Uri uri, string? contentType = null)
    {
        var path = uri.AbsolutePath;
        if (uri.Scheme.Equals("rtmp", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".mpd", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/live/", StringComparison.OrdinalIgnoreCase) ||
            contentType?.Contains("mpegurl", StringComparison.OrdinalIgnoreCase) == true ||
            contentType?.Contains("dash+xml", StringComparison.OrdinalIgnoreCase) == true ||
            contentType?.Contains("x-flv", StringComparison.OrdinalIgnoreCase) == true)
            return RecordKind.Stream;

        return RecordKind.Api;
    }

    public static string? InferFormat(Uri uri, string? contentType = null)
    {
        var path = uri.AbsolutePath;
        if (path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)) return "HLS";
        if (path.EndsWith(".mpd", StringComparison.OrdinalIgnoreCase)) return "DASH";
        if (path.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase)) return "M3U";
        if (uri.Scheme.Equals("rtmp", StringComparison.OrdinalIgnoreCase)) return "RTMP";
        if (contentType?.Contains("x-flv", StringComparison.OrdinalIgnoreCase) == true) return "HTTP-FLV";
        return null;
    }
}
