using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class BusinessWorkingHoursConfiguration : IEntityTypeConfiguration<BusinessWorkingHours>
{
    public void Configure(EntityTypeBuilder<BusinessWorkingHours> builder)
    {
        builder.ToTable("BusinessWorkingHours");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.BusinessId, x.DayOfWeek }).IsUnique();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.WorkingHours)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
