using SharedKernel;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

public record EmaTrendSettings : ValueObject
{
    public int FastPeriod { get; }
    public int SlowPeriod { get; }

    public EmaTrendSettings(int fastPeriod, int slowPeriod)
    {
        if (fastPeriod <= 0)
            throw new DomainException("Fast Ema period must be positive");
        if (slowPeriod <= 0)
            throw new DomainException("Slow Ema period must be positive");

        if (slowPeriod <= fastPeriod)
            throw new DomainException("Slow Ema period must be greater than fast Ema period");

        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
    }
}