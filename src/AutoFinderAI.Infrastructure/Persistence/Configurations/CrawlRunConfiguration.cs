using AutoFinderAI.Domain.Crawling;
using AutoFinderAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFinderAI.Infrastructure.Persistence.Configurations;

public sealed class CrawlRunConfiguration : IEntityTypeConfiguration<CrawlRun>
{
    public void Configure(EntityTypeBuilder<CrawlRun> builder)
    {
        builder.ToTable("CrawlRuns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SourceKey).IsRequired().HasMaxLength(64);

        builder.Property(r => r.Category)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(r => r.Error).HasMaxLength(2048);

        builder.HasIndex(r => r.StartedAt);
    }
}
