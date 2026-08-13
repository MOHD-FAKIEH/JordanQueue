using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameArabic).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEnglish).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DescriptionArabic).HasMaxLength(1000);
        builder.Property(x => x.DescriptionEnglish).HasMaxLength(1000);

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Services)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
