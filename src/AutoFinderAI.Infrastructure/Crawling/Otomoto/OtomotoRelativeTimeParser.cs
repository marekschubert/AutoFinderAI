using System.Globalization;
using System.Text.RegularExpressions;

namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>
/// Parses the Polish "Opublikowano ... temu" phrase found in the listing list page. Only seconds/
/// minutes/hours units mean "less than 24h ago" per the otomoto UI; "wczoraj", "X dni/tydzień/
/// tygodnie temu" (and anything else) mean older, and "Podbite" (bumped, not published) is
/// ignored entirely — it is never a signal of recency.
/// </summary>
public static class OtomotoRelativeTimeParser
{
    private static readonly Regex RecentPattern = new(
        @"Opublikowano\s+(\d+)\s+(sekund[ęy]?|minut[ęy]?|godzin[ęy]?)\s+temu",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>True and an approximate absolute timestamp when the text says "Opublikowano N
    /// second(s)/minute(s)/hour(s) temu" (i.e. published less than 24h ago).</summary>
    public static bool TryParseRecent(string? text, DateTime now, out DateTime publishedAt)
    {
        publishedAt = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var match = RecentPattern.Match(text);
        if (!match.Success)
        {
            return false;
        }

        if (!int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var amount))
        {
            return false;
        }

        var unit = match.Groups[2].Value.ToLowerInvariant();

        var elapsed = unit switch
        {
            var u when u.StartsWith("sekund", StringComparison.Ordinal) => TimeSpan.FromSeconds(amount),
            var u when u.StartsWith("minut", StringComparison.Ordinal) => TimeSpan.FromMinutes(amount),
            var u when u.StartsWith("godzin", StringComparison.Ordinal) => TimeSpan.FromHours(amount),
            _ => (TimeSpan?)null
        };

        if (elapsed is null)
        {
            return false;
        }

        publishedAt = now - elapsed.Value;
        return true;
    }
}
