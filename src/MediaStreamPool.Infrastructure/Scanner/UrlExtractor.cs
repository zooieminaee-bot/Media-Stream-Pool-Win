using System.Text.RegularExpressions;

namespace MediaStreamPool.Infrastructure.Scanner;

public static partial class UrlExtractor
{
    [GeneratedRegex(@"(?:https?|rtmp)://[^\s\""'<>]+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex UrlRegex();

    public static IReadOnlyList<Uri> Extract(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var uris = new List<Uri>();

        foreach (Match match in UrlRegex().Matches(text))
        {
            var candidate = match.Value.TrimEnd('.', ',', ';', ')', ']', '}');
            if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
                continue;
            if (uri.Scheme is not ("http" or "https" or "rtmp"))
                continue;
            if (result.Add(uri.AbsoluteUri))
                uris.Add(uri);
        }

        return uris;
    }
}
