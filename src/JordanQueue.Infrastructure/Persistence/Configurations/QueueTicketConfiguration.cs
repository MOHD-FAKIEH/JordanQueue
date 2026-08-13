using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JordanQueue.Infrastructure.Persistence.Configurations;

public class QueueTicketConfiguration : IEntityTypeConfiguration<QueueTicket>
{
    public void Configure(EntityTypeBuilder<QueueTicket> builder)
    {
        builder.ToTable("QueueTickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasIndex(x => new { x.QueueId, x.Status });
        builder.HasIndex(x => new { x.QueueId, x.TicketNumber }).IsUnique();

        builder.HasOne(x => x.Queue)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.QueueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.QueueTickets)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
