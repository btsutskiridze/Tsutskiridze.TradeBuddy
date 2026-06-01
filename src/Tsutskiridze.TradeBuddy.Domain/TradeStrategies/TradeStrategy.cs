using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

public class TradeStrategy : Entity<int>, IAggregateRoot
{
    public Guid ChatId { get; private set; }
    public TradeStrategyCode Code => TradeStrategyCode.FromId(Id);
    public Timeframe Timeframe { get; private set; }
    public EmaTrendSettings EmaTrend { get; private set; } = null!;
    public AdxTrendStrengthSettings AdxTrendStrength { get; private set; } = null!;
    public AtrStopSettings AtrStop { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private TradeStrategy()
    {
    }

    public TradeStrategy(
        Guid chatId,
        Timeframe timeframe,
        EmaTrendSettings emaTrend,
        AdxTrendStrengthSettings adxTrendStrength,
        AtrStopSettings atrStop)
    {
        if (chatId == Guid.Empty)
            throw new DomainException("Invalid chatId");
        if (!Enum.IsDefined(timeframe))
            throw new DomainException("Invalid timeframe");

        ChatId = chatId;
        Timeframe = timeframe;
        EmaTrend = emaTrend;
        AdxTrendStrength = adxTrendStrength;
        AtrStop = atrStop;
        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            return;
        IsActive = true;
    }

    public bool IsRemovable()
    {
        return !IsActive;
    }
}