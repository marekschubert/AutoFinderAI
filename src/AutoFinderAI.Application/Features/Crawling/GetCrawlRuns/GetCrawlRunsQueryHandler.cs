using AutoFinderAI.Application.Abstractions;
using MediatR;

namespace AutoFinderAI.Application.Features.Crawling.GetCrawlRuns;

public sealed class GetCrawlRunsQueryHandler : IRequestHandler<GetCrawlRunsQuery, IReadOnlyList<CrawlRunDto>>
{
    private readonly ICrawlRunRepository _crawlRunRepository;

    public GetCrawlRunsQueryHandler(ICrawlRunRepository crawlRunRepository)
    {
        _crawlRunRepository = crawlRunRepository;
    }

    public Task<IReadOnlyList<CrawlRunDto>> Handle(GetCrawlRunsQuery request, CancellationToken cancellationToken)
        => _crawlRunRepository.GetRecentAsync(request.Take, cancellationToken);
}
