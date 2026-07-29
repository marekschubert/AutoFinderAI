using AutoFinderAI.Domain.Enums;
using AutoFinderAI.Domain.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFinderAI.Infrastructure.Persistence.Configurations;

public sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.Property(c => c.BodyType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(c => c.DriveType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(c => c.Color).HasMaxLength(64);
        builder.Property(c => c.CountryOfOrigin).HasMaxLength(64);
    }
}
