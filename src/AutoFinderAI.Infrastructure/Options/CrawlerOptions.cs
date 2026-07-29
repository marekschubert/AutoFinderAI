namespace AutoFinderAI.Infrastructure.Options;

public sealed class CrawlerOptions
{
    public const string SectionName = "Crawler";

    public int MaxPages { get; set; } = 2;

    public int RequestDelayMs { get; set; } = 750;

    public string UserAgent { get; set; } =
        "AutoFinderAI-Crawler/1.0 (+https://github.com/AutoFinderAI; contact: dev@autofinderai.local)";

    public int RequestTimeoutSeconds { get; set; } = 30;

    public int MaxRetries { get; set; } = 3;

    /// <summary>Hard safety cap on detail-page fetches per run, independent of MaxPages.</summary>
    public int MaxDetailFetches { get; set; } = 500;
}
