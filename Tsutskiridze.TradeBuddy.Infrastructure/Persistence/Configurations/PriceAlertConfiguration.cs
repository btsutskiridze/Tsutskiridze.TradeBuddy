using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;

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
                .HasColumnType("numeric");

            builder.Property(pa => pa.Direction)
                .IsRequired()
                .HasColumnType("numeric");

            builder.Property(pa => pa.AlertCount)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(pa => pa.CreatedAt)
                .IsRequired();
        }
    }
}
