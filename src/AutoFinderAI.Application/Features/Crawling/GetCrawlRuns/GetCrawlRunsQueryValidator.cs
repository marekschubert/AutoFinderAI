using FluentValidation;

namespace AutoFinderAI.Application.Features.Crawling.GetCrawlRuns;

public sealed class GetCrawlRunsQueryValidator : AbstractValidator<GetCrawlRunsQuery>
{
    public const int MaxTake = 50;

    public GetCrawlRunsQueryValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, MaxTake);
    }
}
