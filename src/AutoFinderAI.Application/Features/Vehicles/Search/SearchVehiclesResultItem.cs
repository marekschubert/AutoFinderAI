namespace AutoFinderAI.Application.Features.Vehicles.Search;

public sealed record SearchVehiclesResultItem(
    Guid Id,
    string Url,
    string Title,
    decimal PriceAmount,
    string PriceCurrency,
    string Make,
    string Model,
    int ProductionYear,
    int? Mileage,
    double Score,
    IReadOnlyList<string> MatchReasons);
