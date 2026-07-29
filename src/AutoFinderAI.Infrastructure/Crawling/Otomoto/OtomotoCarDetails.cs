namespace AutoFinderAI.Infrastructure.Crawling.Otomoto;

/// <summary>Raw (unparsed) attribute strings scraped from a single offer's detail page. See
/// <see cref="OtomotoSelectors"/> for the field-to-selector mapping.</summary>
public sealed record OtomotoCarDetails(
    DateTime? PublishedAt,
    string? Make,
    string? Model,
    string? Year,
    string? Mileage,
    string? FuelType,
    string? Gearbox,
    string? EnginePower,
    string? EngineCapacity,
    string? BodyType,
    string? Drive,
    string? Doors,
    string? Seats,
    string? Color,
    string? CountryOfOrigin,
    string? NoAccident,
    string? OriginalOwner,
    string? PriceAmount,
    string? Currency);
