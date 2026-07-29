using AutoFinderAI.Domain.Enums;
using DriveType = AutoFinderAI.Domain.Enums.DriveType;

namespace AutoFinderAI.Application.Abstractions;

public sealed record VehicleDto(
    Guid Id,
    string Url,
    string Title,
    decimal PriceAmount,
    string PriceCurrency,
    string Make,
    string Model,
    string? Version,
    int ProductionYear,
    int? Mileage,
    FuelType FuelType,
    TransmissionType Transmission,
    int? EnginePowerHp,
    int? EngineCapacityCm3,
    string? Location,
    string? ThumbnailUrl,
    DateTime PublishedAt,
    BodyType BodyType,
    int? Doors,
    int? Seats,
    DriveType? DriveType,
    string? Color,
    bool? IsDamaged,
    bool? IsFirstOwner,
    string? CountryOfOrigin);

/// <summary>Result ordering requested by the user (via AI extraction or the structured search
/// endpoint). Relevance = deterministic ranking score from <see cref="IVehicleRanker"/>.</summary>
public enum VehicleSortBy
{
    Relevance,
    PriceAsc,
    PriceDesc,
    YearDesc,
    MileageAsc
}

/// <summary>Structured, sanitized search criteria. Produced by <see cref="ICriteriaExtractor"/>
/// (AI engineer) or built directly for the vehicle search endpoint. Fields after
/// <see cref="Keywords"/> are additive AI-engineer extensions on top of the original backend
/// contract; <see cref="Make"/>/<see cref="Model"/> stay single-valued to match the existing SQL
/// hard-filter pipeline in <c>VehicleQueries</c> — extra makes/models should be passed as
/// <see cref="Keywords"/> instead of widening this shape to arrays.</summary>
public sealed record VehicleSearchCriteria(
    string? Make = null,
    string? Model = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? MinYear = null,
    int? MaxYear = null,
    int? MaxMileage = null,
    FuelType? FuelType = null,
    TransmissionType? Transmission = null,
    BodyType? BodyType = null,
    int? MinPowerHp = null,
    IReadOnlyList<string>? Keywords = null,
    int? MaxPowerHp = null,
    int? SeatsMin = null,
    bool? ExcludeDamaged = null,
    string? LocationContains = null,
    VehicleSortBy SortBy = VehicleSortBy.Relevance,
    int? Limit = null,
    IReadOnlyList<string>? SoftPreferences = null);

/// <summary>Read-side seam. Implemented in Infrastructure with EF Core: hard filters and sorting
/// happen in SQL, results are capped and projected to <see cref="VehicleDto"/> before returning.</summary>
public interface IVehicleQueries
{
    Task<IReadOnlyList<VehicleDto>> SearchAsync(VehicleSearchCriteria criteria, int candidateCap, CancellationToken cancellationToken);

    Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<VehicleDto>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);
}
