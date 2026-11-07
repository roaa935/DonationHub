using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class PaymentAttemptConfiguration
    : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripeCheckoutSessionId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.StripePaymentIntentId)
            .HasMaxLength(255);

        

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

        builder.Property(x => x.FailureCode)
            .HasMaxLength(100);

        // Donation -> PaymentAttempts
        builder.HasOne<Donation>()
            .WithMany()
            .HasForeignKey(x => x.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.DonationId);

        builder.HasIndex(x => x.StripeCheckoutSessionId)
            .IsUnique();

        builder.HasIndex(x => x.StripePaymentIntentId)
            .IsUnique()
            .HasFilter("[StripePaymentIntentId] IS NOT NULL");
        // Inherited properties from Entity
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");
    }
}