using AutoFinderAI.Domain.Enums;
using AutoFinderAI.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFinderAI.Infrastructure.Persistence.Configurations;

/// <summary>
/// Base configuration for the <see cref="Vehicle"/> TPH hierarchy. The <c>Category</c> column is
/// the discriminator; adding a new subtype only requires a new <c>HasValue&lt;T&gt;</c> mapping here.
/// </summary>
public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        // Vehicle.Category is a computed (read-only, per-subtype) CLR property, not a stored
        // column — ignore it and use a differently-named shadow discriminator column instead
        // ("VehicleType"), since naming the discriminator "Category" collides with the CLR
        // property of the same name (EF tries to bind to it and throws on the type mismatch).
        builder.Ignore(v => v.Category);

        builder.HasDiscriminator<string>("VehicleType")
            .HasValue<Car>(VehicleCategory.Car.ToString());

        builder.Property(v => v.SourceKey).IsRequired().HasMaxLength(64);
        builder.Property(v => v.ExternalId).IsRequired().HasMaxLength(128);
        builder.Property(v => v.Url).IsRequired().HasMaxLength(1024);
        builder.Property(v => v.Title).IsRequired().HasMaxLength(256);
        builder.Property(v => v.Make).IsRequired().HasMaxLength(64);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(64);
        builder.Property(v => v.Version).HasMaxLength(128);
        builder.Property(v => v.Location).HasMaxLength(128);
        builder.Property(v => v.ThumbnailUrl).HasMaxLength(1024);

        builder.Property(v => v.FuelType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(v => v.Transmission)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.OwnsOne(v => v.Price, price =>
        {
            price.Property(p => p.Amount).HasColumnName("Price_Amount").HasPrecision(18, 2);
            price.Property(p => p.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
            price.HasIndex(p => p.Amount);
        });

        builder.HasIndex(v => new { v.SourceKey, v.ExternalId }).IsUnique();
        builder.HasIndex(v => v.PublishedAt);
        builder.HasIndex(v => v.Make);
        builder.HasIndex(v => v.ProductionYear);
    }
}
