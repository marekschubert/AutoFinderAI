using AutoFinderAI.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoFinderAI.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.ModelUsed).HasMaxLength(128);

        builder.HasIndex(m => m.SessionId);
    }
}
