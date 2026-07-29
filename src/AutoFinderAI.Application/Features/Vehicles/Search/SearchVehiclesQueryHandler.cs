using AutoFinderAI.Application.Abstractions;
using MediatR;

namespace AutoFinderAI.Application.Features.Vehicles.Search;

public sealed class SearchVehiclesQueryHandler : IRequestHandler<SearchVehiclesQuery, IReadOnlyList<SearchVehiclesResultItem>>
{
    private readonly IVehicleQueries _vehicleQueries;
    private readonly IVehicleRanker _vehicleRanker;
    private readonly IAiSearchOptions _aiSearchOptions;

    public SearchVehiclesQueryHandler(IVehicleQueries vehicleQueries, IVehicleRanker vehicleRanker, IAiSearchOptions aiSearchOptions)
    {
        _vehicleQueries = vehicleQueries;
        _vehicleRanker = vehicleRanker;
        _aiSearchOptions = aiSearchOptions;
    }

    public async Task<IReadOnlyList<SearchVehiclesResultItem>> Handle(SearchVehiclesQuery request, CancellationToken cancellationToken)
    {
        var criteria = new VehicleSearchCriteria(
            request.Make, request.Model, request.MinPrice, request.MaxPrice, request.MinYear,
            request.MaxYear, request.MaxMileage, request.FuelType, request.Transmission,
            request.BodyType, request.MinPowerHp, null);

        var candidates = await _vehicleQueries.SearchAsync(criteria, _aiSearchOptions.MaxCandidates, cancellationToken);
        var ranked = _vehicleRanker.Rank(candidates, criteria).Take(request.Limit);

        return ranked
            .Select(r => new SearchVehiclesResultItem(
                r.Vehicle.Id, r.Vehicle.Url, r.Vehicle.Title, r.Vehicle.PriceAmount, r.Vehicle.PriceCurrency,
                r.Vehicle.Make, r.Vehicle.Model, r.Vehicle.ProductionYear, r.Vehicle.Mileage, r.Score, r.MatchReasons))
            .ToList();
    }
}
