using AngleSharp.Dom;

namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>Parses a single offer's detail page into raw attribute strings. Selector-driven and
/// defensive: a missing field simply yields null, it never throws.</summary>
public sealed class OtomotoDetailPageParser
{
    private static readonly TimeZoneInfo PolandTimeZone = ResolvePolandTimeZone();

    public OtomotoCarDetails Parse(IDocument document)
        => Parse(document, ParsePublishedAt(document));

    public OtomotoCarDetails Parse(IDocument document, DateTime? publishedAt)
    {
        return new OtomotoCarDetails(
            PublishedAt: publishedAt,
            Make: GetByTestId(document, OtomotoSelectors.Make),
            Model: GetByTestId(document, OtomotoSelectors.Model),
            Year: GetByTestId(document, OtomotoSelectors.Year),
            Mileage: GetByTestId(document, OtomotoSelectors.Mileage) ?? GetByDetailLabel(document, "Przebieg"),
            FuelType: GetByTestId(document, OtomotoSelectors.FuelType) ?? GetByDetailLabel(document, "Rodzaj paliwa"),
            Gearbox: GetByTestId(document, OtomotoSelectors.Gearbox) ?? GetByDetailLabel(document, "Skrzynia biegów"),
            EnginePower: GetByTestId(document, OtomotoSelectors.EnginePower) ?? GetByDetailLabel(document, "Moc"),
            EngineCapacity: GetByTestId(document, OtomotoSelectors.EngineCapacity) ?? GetByDetailLabel(document, "Pojemność skokowa"),
            BodyType: GetByTestId(document, OtomotoSelectors.BodyType) ?? GetByDetailLabel(document, "Typ nadwozia"),
            Drive: GetByTestId(document, OtomotoSelectors.Drive),
            Doors: GetByTestId(document, OtomotoSelectors.DoorCount),
            Seats: GetByTestId(document, OtomotoSelectors.SeatCount),
            Color: GetByTestId(document, OtomotoSelectors.Color),
            CountryOfOrigin: GetByTestId(document, OtomotoSelectors.CountryOfOrigin),
            NoAccident: GetByTestId(document, OtomotoSelectors.NoAccident),
            OriginalOwner: GetByTestId(document, OtomotoSelectors.OriginalOwner),
            PriceAmount: GetByClass(document, OtomotoSelectors.PriceAmountSelector),
            Currency: GetByClass(document, OtomotoSelectors.PriceCurrencySelector));
    }

    public DateTime? ParsePublishedAt(IDocument document)
    {
        foreach (var text in GetPublishedAtCandidates(document))
        {
            if (OtomotoPublishedAtParser.TryParse(text, PolandTimeZone, out var publishedAt))
            {
                return publishedAt;
            }
        }

        return null;
    }

    /// <summary>Reads [data-testid=key] ... p (Label) p (Value) — the value is always the last
    /// p element in the subtree.</summary>
    private static string? GetByTestId(IDocument document, string testId)
    {
        var element = document.QuerySelector(OtomotoSelectors.DataTestId(testId));
        var paragraphs = element?.QuerySelectorAll("p");
        return paragraphs is { Length: > 0 } ? paragraphs[^1].TextContent?.Trim() : null;
    }

    /// <summary>Fallback for the quick-facts cards: div[data-testid="detail"] containing two
    /// sibling p elements — label first, value last.</summary>
    private static string? GetByDetailLabel(IDocument document, string label)
    {
        foreach (var card in document.QuerySelectorAll(OtomotoSelectors.DetailCardSelector))
        {
            var paragraphs = card.QuerySelectorAll("p");
            if (paragraphs.Length >= 2 &&
                string.Equals(paragraphs[0].TextContent?.Trim(), label, StringComparison.OrdinalIgnoreCase))
            {
                return paragraphs[^1].TextContent?.Trim();
            }
        }

        return null;
    }

    /// <summary>Reads the text content of the first element matching a CSS class selector (e.g.
    /// ".offer-price__number"), used for the price block which has no data-testid.</summary>
    private static string? GetByClass(IDocument document, string cssSelector)
        => document.QuerySelector(cssSelector)?.TextContent?.Trim();

    private static IEnumerable<string> GetPublishedAtCandidates(IDocument document)
    {
        var section = document.QuerySelector(OtomotoSelectors.PublishedAtSection);
        var preferredParagraphs = section?.QuerySelectorAll("p").Select(p => p.TextContent?.Trim()) ?? Enumerable.Empty<string?>();

        foreach (var text in preferredParagraphs)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }

        if (section is not null)
        {
            yield break;
        }

        foreach (var text in document.QuerySelectorAll("p").Select(p => p.TextContent?.Trim()))
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    private static TimeZoneInfo ResolvePolandTimeZone()
    {
        foreach (var id in new[] { "Central European Standard Time", "Europe/Warsaw" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
