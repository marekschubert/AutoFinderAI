using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Api.Controllers.Contracts;

public sealed class SearchVehiclesRequest
{
    public string? Make { get; init; }
    public string? Model { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinYear { get; init; }
    public int? MaxYear { get; init; }
    public int? MaxMileage { get; init; }
    public FuelType? FuelType { get; init; }
    public TransmissionType? Transmission { get; init; }
    public BodyType? BodyType { get; init; }
    public int? MinPowerHp { get; init; }
    public int Limit { get; init; } = 20;
}
