using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Features.Crawling.RunCrawl;

public sealed record RunCrawlResult(Guid CrawlRunId, CrawlStatus Status, int ItemsFound, int ItemsSaved, string? Error);
