using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Idempotency;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations;

public sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.KeyHash)
            .HasColumnType("BYTEA")
            .IsRequired();

        builder.Property(x => x.ScopeHash)
            .HasColumnType("BYTEA")
            .IsRequired();

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Scope)
            .HasMaxLength(200);

        builder.Property(x => x.RequestHash)
            .HasColumnType("BYTEA")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ResponseJson);

        builder.Property(x => x.StatusCode);

        builder.Property(x => x.Error)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.CompletedAt);

        builder.Property(x => x.LockedUntil);
        
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
        
        
        builder.HasIndex(x => new { x.KeyHash, x.ScopeHash })
            .IsUnique()
            .HasDatabaseName("UX_idempotency_records_key_scope");
    }
}