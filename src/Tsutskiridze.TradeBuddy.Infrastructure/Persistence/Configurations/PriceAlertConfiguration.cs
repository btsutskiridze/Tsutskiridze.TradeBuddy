using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class PriceAlertConfiguration : IEntityTypeConfiguration<PriceAlert>
    {
        public void Configure(EntityTypeBuilder<PriceAlert> builder)
        {
            builder.ToTable("price_alerts");

            builder.HasKey(pa => pa.Id);

            builder.Property(pa => pa.ChatId)
                .IsRequired();

            builder.Property(pa => pa.StockId)
                .IsRequired();

            builder.OwnsOne(pa => pa.Trigger, trigger =>
            {
                trigger.Property(x => x.Direction)
                    .HasColumnName("direction")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                trigger.Property(x => x.Price)
                    .HasColumnName("price")
                    .HasPrecision(18, 4)
                    .IsRequired();
            });

            builder.Property(pa => pa.AlertCount)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(pa => pa.CreatedAt)
                .IsRequired();

            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.HasOne<Stock>()
                .WithMany()
                .HasForeignKey(pa => pa.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Chat>()
                .WithMany()
                .HasForeignKey(pa => pa.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            // builder.HasIndex(
            //         nameof(PriceAlert.ChatId),
            //         nameof(PriceAlert.StockId),
            //         nameof(PriceAlert.Trigger.Direction),
            //         nameof(PriceAlert.Trigger.Price)
            //     )
            //     .IsUnique()
            //     .HasDatabaseName("UX_price_alerts_chat_stock_direction_price");
        }
    }
}