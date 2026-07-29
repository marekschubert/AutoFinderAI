using FluentValidation;

namespace AutoFinderAI.Application.Features.Vehicles.Search;

public sealed class SearchVehiclesQueryValidator : AbstractValidator<SearchVehiclesQuery>
{
    public const int MaxLimit = 50;

    public SearchVehiclesQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, MaxLimit);

        RuleFor(x => x.MinPrice).LessThanOrEqualTo(x => x.MaxPrice)
            .When(x => x.MinPrice is not null && x.MaxPrice is not null);

        RuleFor(x => x.MinYear).LessThanOrEqualTo(x => x.MaxYear)
            .When(x => x.MinYear is not null && x.MaxYear is not null);
    }
}
