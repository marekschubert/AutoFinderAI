using AutoFinderAI.Domain.Crawling;
using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Abstractions;

public sealed record CrawlRunDto(
    Guid Id,
    string SourceKey,
    VehicleCategory Category,
    DateTime StartedAt,
    DateTime? FinishedAt,
    CrawlStatus Status,
    int ItemsFound,
    int ItemsSaved,
    string? Error);

public interface ICrawlRunRepository
{
    Task AddAsync(CrawlRun run, CancellationToken cancellationToken);

    Task<IReadOnlyList<CrawlRunDto>> GetRecentAsync(int take, CancellationToken cancellationToken);
}
