using AngleSharp.Dom;
using Microsoft.Extensions.Logging;

namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>
/// Parses a search-results page into detail-page links. Recency is intentionally decided after
/// fetching the detail page because otomoto loads the list-page "Opublikowano ..." text later.
/// </summary>
public sealed class OtomotoListPageParser
{
    private readonly ILogger<OtomotoListPageParser>? _logger;

    public OtomotoListPageParser(ILogger<OtomotoListPageParser>? logger = null)
    {
        _logger = logger;
    }

    public IReadOnlyList<OtomotoListItem> Parse(IDocument document, string baseUrl)
    {
        var results = new List<OtomotoListItem>();

        var container = document.QuerySelector(OtomotoSelectors.SearchResultsContainer);
        if (container is null)
        {
            _logger?.LogInformation(
                "otomoto list parser: search results container not found using selector {Selector}.",
                OtomotoSelectors.SearchResultsContainer);
            return results;
        }

        var articles = container.QuerySelectorAll(OtomotoSelectors.ArticleTag);
        _logger?.LogInformation(
            "otomoto list parser: found search results container using selector {ContainerSelector}; article selector {ArticleSelector} matched {ArticleCount} elements.",
            OtomotoSelectors.SearchResultsContainer,
            OtomotoSelectors.ArticleTag,
            articles.Length);

            foreach (var article in articles)
        {
            var externalId = article.GetAttribute("data-id");
            if (string.IsNullOrWhiteSpace(externalId))
            {
                LogArticleDecision(article, externalId, null, null, false, "missing data-id");
                continue;
            }

            var anchor = article.QuerySelector(OtomotoSelectors.TitleLinkSelector)
                ?? article.QuerySelector(OtomotoSelectors.FallbackLinkSelector);

            var href = anchor?.GetAttribute("href");

            // Try to extract a thumbnail URL from the list item (new: user-added element)
            string? thumbnail = null;
            var img = article.QuerySelector(OtomotoSelectors.ThumbnailImageSelector);
            if (img is not null)
            {
                thumbnail = img.GetAttribute("src") ?? img.GetAttribute("data-src");
                if (string.IsNullOrWhiteSpace(thumbnail))
                {
                    thumbnail = null;
                }
            }
            if (string.IsNullOrWhiteSpace(href))
            {
                LogArticleDecision(article, externalId, anchor, href, false, "missing href");
                continue;
            }

            var title = NormalizeText(anchor!.TextContent);
            LogArticleDecision(article, externalId, anchor, href, true, "discovered link");
            var resolvedThumb = string.IsNullOrWhiteSpace(thumbnail) ? null : ResolveUrl(baseUrl, thumbnail!);
            results.Add(new OtomotoListItem(externalId!, ResolveUrl(baseUrl, href!), title ?? string.Empty, resolvedThumb));
        }

        _logger?.LogInformation(
            "otomoto list parser: discovered {DiscoveredCount} listing links from {ArticleCount} article elements.",
            results.Count,
            articles.Length);

        return results;
    }

    private static string ResolveUrl(string baseUrl, string href)
        => Uri.TryCreate(new Uri(baseUrl), href, out var resolved) ? resolved.ToString() : href;

    private void LogArticleDecision(
        IElement article,
        string? externalId,
        IElement? anchor,
        string? href,
        bool accepted,
        string reason)
    {
        if (_logger is null)
        {
            return;
        }

        var allParagraphTexts = article.QuerySelectorAll("p")
            .Select(p => NormalizeText(p.TextContent))
            .OfType<string>()
            .ToArray();

        _logger.LogInformation(
            "otomoto list parser: article decision {DecisionReason}; accepted={Accepted}; data-id={ExternalId}; title={Title}; href={Href}; allParagraphTexts={AllParagraphTexts}; articleHtmlSnippet={ArticleHtmlSnippet}",
            reason,
            accepted,
            externalId,
            NormalizeText(anchor?.TextContent),
            href,
            allParagraphTexts,
            CreateSnippet(article.OuterHtml, 1200));
    }

    private static string? NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string CreateSnippet(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = NormalizeText(value) ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength] + "...";
    }
}
