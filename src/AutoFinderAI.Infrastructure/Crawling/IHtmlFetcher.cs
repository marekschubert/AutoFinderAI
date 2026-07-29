namespace AutoFinderAI.Infrastructure.Crawling;

/// <summary>Fetches raw HTML for a URL. Returns null (never throws) on failure so a single bad
/// request never aborts a crawl run.</summary>
public interface IHtmlFetcher
{
    Task<string?> FetchAsync(string url, CancellationToken cancellationToken);
}
