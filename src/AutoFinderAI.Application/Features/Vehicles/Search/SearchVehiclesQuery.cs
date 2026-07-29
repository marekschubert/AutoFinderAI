using AutoFinderAI.Domain.Enums;
using MediatR;

namespace AutoFinderAI.Application.Features.Vehicles.Search;

public sealed record SearchVehiclesQuery(
    string? Make,
    string? Model,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinYear,
    int? MaxYear,
    int? MaxMileage,
    FuelType? FuelType,
    TransmissionType? Transmission,
    BodyType? BodyType,
    int? MinPowerHp,
    int Limit) : IRequest<IReadOnlyList<SearchVehiclesResultItem>>;
