using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class ProcessedStripeEventConfiguration
    : IEntityTypeConfiguration<ProcessedStripeEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedStripeEvent> builder)
    {
        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.ProcessedAt)
            .IsRequired();
    }
}