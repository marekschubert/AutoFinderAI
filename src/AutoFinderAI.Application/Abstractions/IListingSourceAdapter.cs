using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Application.Abstractions;

/// <summary>
/// Raw scraped listing DTO: plain strings straight off the source HTML, not yet culture-parsed or
/// enum-mapped (that happens in <c>RawListingToCarMapper</c>). A missing/unselectable field is
/// simply null here — the mapper is responsible for defensive fallback, never the parser.
/// </summary>
public sealed record RawListing(
    string ExternalId,
    string Url,
    string Title,
    DateTime PublishedAt,
    string? PriceText,
    string? CurrencyText,
    string? MakeText,
    string? ModelText,
    string? YearText,
    string? MileageText,
    string? FuelTypeText,
    string? TransmissionText,
    string? EnginePowerText,
    string? EngineCapacityText,
    string? BodyTypeText,
    string? DriveTypeText,
    string? DoorsText,
    string? SeatsText,
    string? ColorText,
    string? CountryOfOriginText,
    string? NoAccidentText,
    string? OriginalOwnerText,
    string? LocationText,
    string? ThumbnailUrl);

/// <summary>Seam implemented per data source (e.g. otomoto.pl) by the backend engineer.</summary>
public interface IListingSourceAdapter
{
    string SourceKey { get; }

    IReadOnlyCollection<VehicleCategory> Supported { get; }

    IAsyncEnumerable<RawListing> CrawlAsync(VehicleCategory category, CancellationToken cancellationToken);
}
