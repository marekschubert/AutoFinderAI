using AutoFinderAI.Domain.Enums;
using DriveType = AutoFinderAI.Domain.Enums.DriveType;

namespace AutoFinderAI.Domain.Vehicles;

public sealed class Car : Vehicle
{
    public BodyType BodyType { get; private set; }
    public int? Doors { get; private set; }
    public int? Seats { get; private set; }
    public DriveType? DriveType { get; private set; }
    public string? Color { get; private set; }
    public bool? IsDamaged { get; private set; }
    public bool? IsFirstOwner { get; private set; }
    public string? CountryOfOrigin { get; private set; }

    public override VehicleCategory Category => VehicleCategory.Car;

    private Car()
    {
    }

    private Car(
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
        DateTime scrapedAt,
        BodyType bodyType,
        int? doors,
        int? seats,
        DriveType? driveType,
        string? color,
        bool? isDamaged,
        bool? isFirstOwner,
        string? countryOfOrigin)
        : base(
            id, sourceKey, externalId, url, title, price, make, model, version, productionYear,
            mileage, fuelType, transmission, enginePowerHp, engineCapacityCm3, location,
            thumbnailUrl, publishedAt, scrapedAt)
    {
        BodyType = bodyType;
        Doors = doors;
        Seats = seats;
        DriveType = driveType;
        Color = color;
        IsDamaged = isDamaged;
        IsFirstOwner = isFirstOwner;
        CountryOfOrigin = countryOfOrigin;
    }

    public static Car Create(
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
        DateTime scrapedAt,
        BodyType bodyType,
        int? doors = null,
        int? seats = null,
        DriveType? driveType = null,
        string? color = null,
        bool? isDamaged = null,
        bool? isFirstOwner = null,
        string? countryOfOrigin = null)
    {
        return new Car(
            Guid.NewGuid(), sourceKey, externalId, url, title, price, make, model, version,
            productionYear, mileage, fuelType, transmission, enginePowerHp, engineCapacityCm3,
            location, thumbnailUrl, publishedAt, scrapedAt, bodyType, doors, seats, driveType,
            color, isDamaged, isFirstOwner, countryOfOrigin);
    }

    /// <summary>
    /// Refreshes mutable fields when the crawler finds the same listing again (upsert on
    /// (SourceKey, ExternalId)). Identity/category fields never change.
    /// </summary>
    public void UpdateFromSource(
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
        DateTime scrapedAt,
        BodyType bodyType,
        int? doors,
        int? seats,
        DriveType? driveType,
        string? color,
        bool? isDamaged,
        bool? isFirstOwner,
        string? countryOfOrigin)
    {
        RefreshCore(
            title, price, mileage, fuelType, transmission, enginePowerHp, engineCapacityCm3,
            location, thumbnailUrl, publishedAt, scrapedAt);

        BodyType = bodyType;
        Doors = doors;
        Seats = seats;
        DriveType = driveType;
        Color = color;
        IsDamaged = isDamaged;
        IsFirstOwner = isFirstOwner;
        CountryOfOrigin = countryOfOrigin;
    }
}
