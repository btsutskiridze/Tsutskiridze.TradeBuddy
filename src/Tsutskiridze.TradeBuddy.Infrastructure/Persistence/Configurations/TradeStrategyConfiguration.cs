using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Configurations
{
    public class TradeStrategyConfiguration : IEntityTypeConfiguration<TradeStrategy>
    {
        public void Configure(EntityTypeBuilder<TradeStrategy> builder)
        {
            builder.ToTable("trade_strategies");

            builder.HasKey(ts => ts.Id);

            builder.Property(ts => ts.ChatId)
                .IsRequired();

            builder.Property(ts => ts.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(ts => ts.Timeframe)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(ts => ts.IsActive)
                .IsRequired();

            builder.OwnsOne(ts => ts.EmaTrend, emaTrend =>
            {
                emaTrend.Property(x => x.FastPeriod)
                    .HasColumnName("ema_fast_period")
                    .IsRequired();

                emaTrend.Property(x => x.SlowPeriod)
                    .HasColumnName("ema_slow_period")
                    .IsRequired();
            });

            builder.Navigation(ts => ts.EmaTrend)
                .IsRequired();

            builder.OwnsOne(ts => ts.AdxTrendStrength, adxTrendStrength =>
            {
                adxTrendStrength.Property(x => x.Period)
                    .HasColumnName("adx_period")
                    .IsRequired();

                adxTrendStrength.Property(x => x.TrendStrengthThreshold)
                    .HasColumnName("adx_trend_strength_threshold")
                    .HasPrecision(18, 4)
                    .IsRequired();

                adxTrendStrength.Property(x => x.NonFallingLookBackBars)
                    .HasColumnName("adx_non_falling_look_back_bars")
                    .IsRequired();
            });

            builder.Navigation(ts => ts.AdxTrendStrength)
                .IsRequired();

            builder.OwnsOne(ts => ts.AtrStop, atrStop =>
            {
                atrStop.Property(x => x.Period)
                    .HasColumnName("atr_period")
                    .IsRequired();

                atrStop.Property(x => x.InitialStopMultiplier)
                    .HasColumnName("atr_initial_stop_multiplier")
                    .HasPrecision(18, 4)
                    .IsRequired();

                atrStop.Property(x => x.TrailingStopMultiplier)
                    .HasColumnName("atr_trailing_stop_multiplier")
                    .HasPrecision(18, 4)
                    .IsRequired();

                atrStop.Property(x => x.TrailingActivationMultiplier)
                    .HasColumnName("atr_trailing_activation_multiplier")
                    .HasPrecision(18, 4)
                    .IsRequired();
            });

            builder.Navigation(ts => ts.AtrStop)
                .IsRequired();

            builder.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            builder.HasOne<Chat>()
                .WithMany()
                .HasForeignKey(ts => ts.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ts => ts.ChatId)
                .HasDatabaseName("IX_trade_strategies_chat_id");

            builder.HasIndex(ts => new { ts.ChatId, ts.Name })
                .IsUnique()
                .HasDatabaseName("UX_trade_strategies_chat_name");
        }
    }
}
