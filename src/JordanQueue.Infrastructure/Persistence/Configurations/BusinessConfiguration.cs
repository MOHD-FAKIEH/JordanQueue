using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameArabic).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEnglish).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DescriptionArabic).HasMaxLength(1000);
        builder.Property(x => x.DescriptionEnglish).HasMaxLength(1000);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.AddressArabic).HasMaxLength(500);
        builder.Property(x => x.AddressEnglish).HasMaxLength(500);
        builder.Property(x => x.Category).HasConversion<int>();

        builder.HasIndex(x => x.OwnerUserId);

        builder.HasOne(x => x.Owner)
            .WithMany(x => x.OwnedBusinesses)
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
