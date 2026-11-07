using DonationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DonationHub.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration: IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameAr)
            .IsRequired()
            .HasMaxLength(100);


        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.NameAr)
           .IsUnique();

        builder.HasIndex(c => c.NameEn)
            .IsUnique();

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(1000).IsRequired(false);

        builder.Property(x => x.DescriptionEn)
            .HasMaxLength(1000).IsRequired(false);

       
    
        // Inherited properties from Entity
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        // One Category -> Many Campaigns
        builder.HasMany(x => x.Campaigns)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index
        builder.HasIndex(x => x.NameEn);
    }
}