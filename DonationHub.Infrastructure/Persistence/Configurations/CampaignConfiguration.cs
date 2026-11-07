using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration
    : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TitleAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.TitleEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.DescriptionAr)
            .IsRequired();

        builder.Property(x => x.DescriptionEn)
            .IsRequired();

        builder.OwnsOne(x => x.GoalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("GoalAmount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.OwnsOne(x => x.RaisedAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("RaisedAmount")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.Property(x => x.BannerFileName)
            .HasMaxLength(255);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnType("datetime2")
            .IsRequired(false);


        builder.Property(x => x.IsDeleted)
            .IsRequired();

        // Category -> Campaigns
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Campaigns)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // User -> Campaigns
        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.Campaigns)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.CreatedByUserId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.IsDeleted);
        builder.Property(x => x.RowVersion).IsRowVersion();
        // Inherited properties from Entity
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}