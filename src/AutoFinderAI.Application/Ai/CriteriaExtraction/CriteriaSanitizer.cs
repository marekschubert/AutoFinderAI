using AutoFinderAI.Application.Abstractions;
using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Ai.CriteriaExtraction;

/// <summary>
/// Never trusts the LLM: enum whitelisting, numeric range clamping/normalisation, collection
/// deduplication and limit clamping all happen here before a <see cref="RawCriteriaDto"/> becomes
/// a <see cref="VehicleSearchCriteria"/>.
/// </summary>
public static class CriteriaSanitizer
{
    private const int MinYearBound = 1950;
    private const decimal MaxPriceBound = 10_000_000m;
    private const int MaxMileageBound = 2_000_000;
    private const int MaxPowerBound = 2000;
    private const int MaxSeatsBound = 9;
    private const int MaxListItems = 10;

    public static (VehicleSearchCriteria? Criteria, string? Intro) Sanitize(RawCriteriaDto raw, IAiSearchOptions options)
    {
        var make = Clean(raw.Make);
        var model = Clean(raw.Model);

        var yearFrom = ClampYear(raw.YearFrom);
        var yearTo = ClampYear(raw.YearTo);
        if (yearFrom is not null && yearTo is not null && yearFrom > yearTo)
        {
            (yearFrom, yearTo) = (yearTo, yearFrom);
        }

        var priceFrom = ClampPrice(raw.PriceFrom);
        var priceTo = ClampPrice(raw.PriceTo);
        if (priceFrom is not null && priceTo is not null && priceFrom > priceTo)
        {
            (priceFrom, priceTo) = (priceTo, priceFrom);
        }

        var powerFrom = ClampPower(raw.EnginePowerHpFrom);
        var powerTo = ClampPower(raw.EnginePowerHpTo);
        if (powerFrom is not null && powerTo is not null && powerFrom > powerTo)
        {
            (powerFrom, powerTo) = (powerTo, powerFrom);
        }

        var mileageMax = ClampMileage(raw.MileageMax);
        var seatsMin = ClampSeats(raw.SeatsMin);
        var location = Clean(raw.LocationContains);

        var fuelType = ParseEnum<FuelType>(raw.FuelType);
        var transmission = ParseEnum<TransmissionType>(raw.Transmission);
        var bodyType = ParseEnum<BodyType>(raw.BodyType);
        var sortBy = ParseEnum<VehicleSortBy>(raw.SortBy) ?? VehicleSortBy.Relevance;

        var keywords = Dedupe(raw.Keywords);
        var softPreferences = Dedupe(raw.SoftPreferences);

        var limit = raw.Limit is int requestedLimit
            ? Math.Clamp(requestedLimit, 1, options.MaxLimit)
            : options.DefaultLimit;

        var hasAnyFilter = make is not null || model is not null || yearFrom is not null || yearTo is not null
            || priceFrom is not null || priceTo is not null || mileageMax is not null || fuelType is not null
            || transmission is not null || bodyType is not null || powerFrom is not null || powerTo is not null
            || seatsMin is not null || location is not null || keywords.Count > 0 || softPreferences.Count > 0
            || raw.ExcludeDamaged is true;

        if (!hasAnyFilter)
        {
            return (null, Clean(raw.Intro));
        }

        var criteria = new VehicleSearchCriteria(
            Make: make,
            Model: model,
            MinPrice: priceFrom,
            MaxPrice: priceTo,
            MinYear: yearFrom,
            MaxYear: yearTo,
            MaxMileage: mileageMax,
            FuelType: fuelType,
            Transmission: transmission,
            BodyType: bodyType,
            MinPowerHp: powerFrom,
            Keywords: keywords.Count > 0 ? keywords : null,
            MaxPowerHp: powerTo,
            SeatsMin: seatsMin,
            ExcludeDamaged: raw.ExcludeDamaged,
            LocationContains: location,
            SortBy: sortBy,
            Limit: limit,
            SoftPreferences: softPreferences.Count > 0 ? softPreferences : null);

        return (criteria, Clean(raw.Intro));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ClampYear(int? year) => year is null ? null : Math.Clamp(year.Value, MinYearBound, DateTime.UtcNow.Year + 1);

    private static decimal? ClampPrice(decimal? price) => price is null ? null : Math.Clamp(price.Value, 0, MaxPriceBound);

    private static int? ClampMileage(int? mileage) => mileage is null ? null : Math.Clamp(mileage.Value, 0, MaxMileageBound);

    private static int? ClampPower(int? power) => power is null ? null : Math.Clamp(power.Value, 0, MaxPowerBound);

    private static int? ClampSeats(int? seats) => seats is null ? null : Math.Clamp(seats.Value, 1, MaxSeatsBound);

    private static T? ParseEnum<T>(string? value) where T : struct, Enum
        => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : null;

    private static List<string> Dedupe(List<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return new List<string>();
        }

        return values
            .Select(v => v?.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxListItems)
            .ToList();
    }
}
