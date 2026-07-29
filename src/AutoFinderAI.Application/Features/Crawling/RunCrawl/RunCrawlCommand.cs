using AutoFinderAI.Application.Common;
using AutoFinderAI.Domain.Enums;
using MediatR;

namespace AutoFinderAI.Application.Features.Crawling.RunCrawl;

public sealed record RunCrawlCommand(VehicleCategory Category) : IRequest<Result<RunCrawlResult>>;
