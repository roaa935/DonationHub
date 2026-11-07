using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class IdempotencyRecordConfiguration
    : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.RequestHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ResponseBody)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ResponseStatusCode)
            .IsRequired();

        // User -> IdempotencyRecords
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Key })
            .IsUnique();

        builder.HasIndex(x => x.ExpiresAt);
    }
}