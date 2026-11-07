using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class DonationConfiguration
    : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.HasKey(x => x.Id);

        

        builder.OwnsOne(x => x.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(1000);

        builder.Property(x => x.ReceiptNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ReceiptFileName)
            .HasMaxLength(255);

        // Campaign -> Donations
        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Donations
        builder.HasOne(x => x.DonorUser)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.DonorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CampaignId);

        builder.HasIndex(x => x.DonorUserId);

        builder.HasIndex(x => x.ReceiptNumber)
            .IsUnique()
            .HasFilter("[ReceiptNumber] IS NOT NULL");
        // Inherited properties from Entity
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");
    }
}