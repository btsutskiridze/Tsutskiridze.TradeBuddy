using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

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

            builder.Property(pa => pa.Price)
                .IsRequired()
                .HasPrecision(18, 4);

            builder.Property(pa => pa.Direction)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

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
            
            builder.HasIndex(pa => new
                {
                    pa.ChatId,
                    pa.StockId,
                    pa.Direction,
                    pa.Price
                })
                .IsUnique()
                .HasDatabaseName("UX_price_alerts_chat_stock_direction_price");

        }
    }
}
