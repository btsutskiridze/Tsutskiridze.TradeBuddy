using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Outbox;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.EventVersion)
            .IsRequired();

        builder.Property(x => x.Payload)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(x => x.Headers)
            .HasColumnType("text");

        builder.Property(x => x.OccurTime)
            .IsRequired();

        builder.Property(x => x.CreateTime)
            .IsRequired();

        builder.Property(x => x.ProcessDate)
            .IsRequired(false);

        builder.Property(x => x.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.NextRetryTime)
            .IsRequired(false);

        builder.Property(x => x.LastError)
            .HasMaxLength(4000);

        builder.Ignore(x => x.IsProcessed);
        builder.Ignore(x => x.IsReadyToProcess);

        builder.HasIndex(x => new { x.ProcessDate, x.NextRetryTime, x.CreateTime })
            .HasDatabaseName("IX_outbox_messages_pending_dispatch");
    }
}
