using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class BusinessStaffConfiguration : IEntityTypeConfiguration<BusinessStaff>
{
    public void Configure(EntityTypeBuilder<BusinessStaff> builder)
    {
        builder.ToTable("BusinessStaff");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.BusinessId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Staff)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(x => x.StaffAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
