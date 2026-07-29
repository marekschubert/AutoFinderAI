using AutoFinderAI.Domain.Enums;

namespace AutoFinderAI.Domain.Vehicles;

/// <summary>
/// Aggregate root for a crawled listing. Mapped via EF Core TPH with <see cref=""Category""/>
/// as the discriminator. Adding a new category (e.g. Motorcycle) requires only a new subclass
/// + enum value + source adapter.
/// </summary>
public abstract class Vehicle
{
    public Guid Id { get; private set; }
    public string SourceKey { get; private set; } = default!;
    public string ExternalId { get; private set; } = default!;
    public string Url { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public Money Price { get; private set; } = default!;
    public string Make { get; private set; } = default!;
    public string Model { get; private set; } = default!;
    public string? Version { get; private set; }
    public int ProductionYear { get; private set; }
    public int? Mileage { get; private set; }
    public FuelType FuelType { get; private set; }
    public TransmissionType Transmission { get; private set; }
    public int? EnginePowerHp { get; private set; }
    public int? EngineCapacityCm3 { get; private set; }
    public string? Location { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public DateTime ScrapedAt { get; private set; }

    public abstract VehicleCategory Category { get; }

    protected Vehicle()
    {
    }

    protected Vehicle(
        Guid id,
        string sourceKey,
        string externalId,
        string url,
        string title,
        Money price,
        string make,
        string model,
        string? version,
        int productionYear,
        int? mileage,
        FuelType fuelType,
        TransmissionType transmission,
        int? enginePowerHp,
        int? engineCapacityCm3,
        string? location,
        string? thumbnailUrl,
        DateTime publishedAt,
        DateTime scrapedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            throw new ArgumentException("SourceKey is required.", nameof(sourceKey));
        }

        if (string.IsNullOrWhiteSpace(externalId))
        {
            throw new ArgumentException("ExternalId is required.", nameof(externalId));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Url is required.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(make))
        {
            throw new ArgumentException("Make is required.", nameof(make));
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model is required.", nameof(model));
        }

        if (productionYear < 1900)
        {
            throw new ArgumentOutOfRangeException(nameof(productionYear), "ProductionYear looks invalid.");
        }

        Id = id;
        SourceKey = sourceKey;
        ExternalId = externalId;
        Url = url;
        Title = title;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Make = make;
        Model = model;
        Version = version;
        ProductionYear = productionYear;
        Mileage = mileage;
        FuelType = fuelType;
        Transmission = transmission;
        EnginePowerHp = enginePowerHp;
        EngineCapacityCm3 = engineCapacityCm3;
        Location = location;
        ThumbnailUrl = thumbnailUrl;
        PublishedAt = publishedAt;
        ScrapedAt = scrapedAt;
    }

    /// <summary>
    /// Refreshes the mutable, source-controlled fields when a re-crawl finds the same
    /// (SourceKey, ExternalId) listing again. Identity fields (Id, SourceKey, ExternalId, Make,
    /// Model, ProductionYear) never change on update.
    /// </summary>
    protected void RefreshCore(
        string title,
        Money price,
        int? mileage,
        FuelType fuelType,
        TransmissionType transmission,
        int? enginePowerHp,
        int? engineCapacityCm3,
        string? location,
        string? thumbnailUrl,
        DateTime publishedAt,
        DateTime scrapedAt)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Title = title;
        }

        Price = price ?? throw new ArgumentNullException(nameof(price));
        Mileage = mileage;
        FuelType = fuelType;
        Transmission = transmission;
        EnginePowerHp = enginePowerHp;
        EngineCapacityCm3 = engineCapacityCm3;
        Location = location;
        ThumbnailUrl = thumbnailUrl;
        PublishedAt = publishedAt;
        ScrapedAt = scrapedAt;
    }
}
