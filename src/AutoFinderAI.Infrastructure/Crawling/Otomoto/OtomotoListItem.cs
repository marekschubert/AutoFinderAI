namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>One listing link discovered on a search-results page. Recency is decided after
/// fetching the detail page, where the full publication date is available immediately.</summary>
/// <summary>
/// One listing link discovered on a search-results page. Recency is decided after
/// fetching the detail page, where the full publication date is available immediately.
/// Now includes an optional `ThumbnailUrl` discovered on the list page.
/// </summary>
public sealed record OtomotoListItem(string ExternalId, string Url, string Title, string? ThumbnailUrl);
