using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Outbox;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventVersion)
            .IsRequired();

        builder.Property(x => x.SerializeType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Headers);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Status,
            x.NextRetryAt,
            x.OccurredAt
        });

        builder.HasIndex(x => x.LockId)
            .HasFilter("lock_id IS NOT NULL");

        builder.HasIndex(x => x.DeadAt)
            .HasFilter("dead_at IS NOT NULL");
    }
}