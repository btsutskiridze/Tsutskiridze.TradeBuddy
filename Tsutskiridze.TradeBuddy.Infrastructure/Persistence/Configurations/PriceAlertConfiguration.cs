using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class PriceAlertConfiguration : IEntityTypeConfiguration<PriceAlert>
    {
        public void Configure(EntityTypeBuilder<PriceAlert> builder)
        {
            builder.ToTable("price_alerts");

            builder.HasKey(pa => pa.ID);

            builder.Property(pa => pa.ChatID)
                .IsRequired();

            builder.Property(pa => pa.StockID)
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
