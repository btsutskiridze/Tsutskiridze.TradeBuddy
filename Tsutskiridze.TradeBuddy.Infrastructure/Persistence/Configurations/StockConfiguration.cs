using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Core.DBEntities.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stocks");

            builder.HasKey(s => s.ID);

            builder.Property(s => s.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasMany(s => s.PriceAlerts)
                .WithOne(pa => pa.Stock)
                .HasForeignKey(pa => pa.StockID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
