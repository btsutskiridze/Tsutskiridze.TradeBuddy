using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;

public class StrategyMonitor : Entity<Guid>, IAggregateRoot
{
    public Guid ChatId { get; private set; }
    public Guid TradeStrategyId { get; private set; }
    public Guid StockId { get; private set; }
    public string Symbol { get; private set; } = null!;
    public Timeframe Timeframe { get; private set; }
    public MonitorStatus Status { get; private set; }
    public StrategyPositionState PositionState { get; private set; }
    public DateTime CreateTime { get; private set; }
    public DateTime? StopTime { get; private set; }

    private StrategyMonitor() {}
    
    public StrategyMonitor(
        Guid id,
        Guid chatId,
        Guid tradeStrategyId,
        Guid stockId,
        string symbol,
        Timeframe timeframe,
        DateTime nowUtc) : base(id)
    {
        if(Guid.Empty == id) throw new DomainException("Invalid id");
        if (chatId == Guid.Empty) throw new DomainException("Invalid chatId");
        if (tradeStrategyId == Guid.Empty) throw new DomainException("Invalid tradeStrategyId");
        if (stockId == Guid.Empty) throw new DomainException("Invalid stockId");
        if (string.IsNullOrEmpty(symbol)) throw new DomainException("Invalid symbol");
        if (!Enum.IsDefined(timeframe)) throw new DomainException("Invalid timeframe");

        ChatId = chatId;
        TradeStrategyId = tradeStrategyId;
        StockId = stockId;
        Symbol = symbol.Trim().ToUpperInvariant();
        Timeframe = timeframe;
        Status = MonitorStatus.Active;
        PositionState = StrategyPositionState.OutOfMarket();
        CreateTime = nowUtc;
    }
}