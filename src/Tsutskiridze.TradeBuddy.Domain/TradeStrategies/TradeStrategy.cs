using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

public class TradeStrategy : Entity<Guid>, IAggregateRoot
{
    public Guid ChatId { get; private set; }
    public string Name { get; private set; } = null!;
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
        string name, 
        Timeframe timeframe, 
        EmaTrendSettings emaTrend,
        AdxTrendStrengthSettings adxTrendStrength, 
        AtrStopSettings atrStop)
    {
        if (chatId == Guid.Empty)
            throw new DomainException("Invalid chatId");
        if (string.IsNullOrEmpty(name))
            throw new DomainException("Invalid name");
        if (!Enum.IsDefined(timeframe))
            throw new DomainException("Invalid timeframe");

        ChatId = chatId;
        Name = name;
        Timeframe = timeframe;
        EmaTrend = emaTrend;
        AdxTrendStrength = adxTrendStrength;
        AtrStop = atrStop;
        IsActive = true;
    }
}