using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.Stocks;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class StrategyMonitorConfiguration : IEntityTypeConfiguration<StrategyMonitor>
    {
        public void Configure(EntityTypeBuilder<StrategyMonitor> builder)
        {
            builder.ToTable("strategy_monitors");

            builder.HasKey(sm => sm.Id);

            builder.Property(sm => sm.Id)
                .ValueGeneratedOnAdd();

            builder.Property(sm => sm.ChatId)
                .IsRequired();

            builder.Property(sm => sm.TradeStrategyId)
                .IsRequired();

            builder.Property(sm => sm.StockId)
                .IsRequired();

            builder.Property(sm => sm.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sm => sm.Timeframe)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(sm => sm.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.OwnsOne(sm => sm.PositionState, positionState =>
            {
                positionState.Property(x => x.Side)
                    .HasColumnName("position_side")
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                positionState.Property(x => x.EntryPrice)
                    .HasColumnName("entry_price")
                    .HasPrecision(18, 4);

                positionState.Property(x => x.EntryDate)
                    .HasColumnName("entry_date");

                positionState.Property(x => x.LockedAtr)
                    .HasColumnName("locked_atr")
                    .HasPrecision(18, 4);

                positionState.Property(x => x.HighestClose)
                    .HasColumnName("highest_close")
                    .HasPrecision(18, 4);

                positionState.Property(x => x.ActiveStop)
                    .HasColumnName("active_stop")
                    .HasPrecision(18, 4);

                positionState.Property(x => x.TrailingActivated)
                    .HasColumnName("trailing_activated")
                    .IsRequired();
            });

            builder.Navigation(sm => sm.PositionState)
                .IsRequired();

            builder.Property(sm => sm.CreateTime)
                .IsRequired();

            builder.Property(sm => sm.StopTime)
                .IsRequired(false);

            builder.Property(sm => sm.LastEvaluatedCandleDate)
                .IsRequired(false);

            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.HasOne<Chat>()
                .WithMany()
                .HasForeignKey(sm => sm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<TradeStrategy>()
                .WithMany()
                .HasForeignKey(sm => sm.TradeStrategyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Stock>()
                .WithMany()
                .HasForeignKey(sm => sm.StockId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(sm => new { sm.ChatId, sm.TradeStrategyId, sm.StockId })
                .IsUnique()
                .HasDatabaseName("ux_strategy_monitors_active_chat_strategy_stock");
        }
    }
}
