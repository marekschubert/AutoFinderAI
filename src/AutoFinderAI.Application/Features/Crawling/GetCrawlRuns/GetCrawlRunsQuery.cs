using AutoFinderAI.Application.Abstractions;
using MediatR;

namespace AutoFinderAI.Application.Features.Crawling.GetCrawlRuns;

public sealed record GetCrawlRunsQuery(int Take) : IRequest<IReadOnlyList<CrawlRunDto>>;
