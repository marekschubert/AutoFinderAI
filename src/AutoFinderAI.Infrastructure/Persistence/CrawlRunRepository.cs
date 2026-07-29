using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Crawling;
using Microsoft.EntityFrameworkCore;

namespace AutoFinderAI.Infrastructure.Persistence;

public sealed class CrawlRunRepository : ICrawlRunRepository
{
    private readonly AppDbContext _dbContext;

    public CrawlRunRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CrawlRun run, CancellationToken cancellationToken)
        => await _dbContext.CrawlRuns.AddAsync(run, cancellationToken);

    public async Task<IReadOnlyList<CrawlRunDto>> GetRecentAsync(int take, CancellationToken cancellationToken)
    {
        return await _dbContext.CrawlRuns.AsNoTracking()
            .OrderByDescending(r => r.StartedAt)
            .Take(take)
            .Select(r => new CrawlRunDto(
                r.Id, r.SourceKey, r.Category, r.StartedAt, r.FinishedAt, r.Status, r.ItemsFound, r.ItemsSaved, r.Error))
            .ToListAsync(cancellationToken);
    }
}
