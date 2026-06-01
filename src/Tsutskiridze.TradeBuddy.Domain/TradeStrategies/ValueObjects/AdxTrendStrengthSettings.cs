using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

public record AdxTrendStrengthSettings : ValueObject
{
    public int Period { get; }
    public decimal TrendStrengthThreshold { get; }
    public int NonFallingLookBackBars { get; }

    public AdxTrendStrengthSettings(int period, decimal trendStrengthThreshold, int nonFallingLookBackBars)
    {
        if (period <= 0)
            throw new DomainException("Period must be positive");

        if (trendStrengthThreshold <= 0)
            throw new DomainException("Trend strength threshold must be positive");

        if (nonFallingLookBackBars <= 0)
            throw new DomainException("Non falling look back bars must be positive");

        Period = period;
        TrendStrengthThreshold = trendStrengthThreshold;
        NonFallingLookBackBars = nonFallingLookBackBars;
    }
}