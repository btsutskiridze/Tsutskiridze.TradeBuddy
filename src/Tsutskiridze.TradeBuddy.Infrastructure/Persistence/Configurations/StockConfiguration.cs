using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.ToTable("stocks");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
            
            builder.HasIndex(x => x.Symbol)
                .IsUnique()
                .HasDatabaseName("UX_stocks_symbol");
        }
    }
}

