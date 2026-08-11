using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class QueueConfiguration : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.ToTable("Queues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => new { x.BusinessId, x.QueueDate });
        builder.HasIndex(x => new { x.BusinessId, x.ServiceId, x.QueueDate }).IsUnique();

        builder.HasOne(x => x.Business)
            .WithMany(x => x.Queues)
            .HasForeignKey(x => x.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Queues)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
