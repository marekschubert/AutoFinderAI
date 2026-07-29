using System.Text.RegularExpressions;

namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

public static class OtomotoPublishedAtParser
{
    private static readonly Regex DatePattern = new(
        @"^\s*(\d{1,2})\s+([A-Za-ząćęłńóśźżĄĆĘŁŃÓŚŹŻ]+)\s+(\d{4})\s+(\d{1,2}):(\d{2})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyDictionary<string, int> PolishMonths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["stycznia"] = 1,
        ["lutego"] = 2,
        ["marca"] = 3,
        ["kwietnia"] = 4,
        ["maja"] = 5,
        ["czerwca"] = 6,
        ["lipca"] = 7,
        ["sierpnia"] = 8,
        ["września"] = 9,
        ["pazdziernika"] = 10,
        ["października"] = 10,
        ["listopada"] = 11,
        ["grudnia"] = 12
    };

    public static bool TryParse(string? text, TimeZoneInfo sourceTimeZone, out DateTime publishedAtUtc)
    {
        publishedAtUtc = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var match = DatePattern.Match(NormalizeText(text));
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups[1].Value, out var day) ||
            !PolishMonths.TryGetValue(match.Groups[2].Value, out var month) ||
            !int.TryParse(match.Groups[3].Value, out var year) ||
            !int.TryParse(match.Groups[4].Value, out var hour) ||
            !int.TryParse(match.Groups[5].Value, out var minute))
        {
            return false;
        }

        try
        {
            var localPublishedAt = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
            publishedAtUtc = TimeZoneInfo.ConvertTimeToUtc(localPublishedAt, sourceTimeZone);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string NormalizeText(string text)
        => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
