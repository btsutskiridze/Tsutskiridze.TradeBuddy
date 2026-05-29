using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Events;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;

public class StrategyMonitor : Entity<int>, IAggregateRoot
{
    public Guid ChatId { get; private set; }
    public int TradeStrategyId { get; private set; }
    public Guid StockId { get; private set; }
    public string Symbol { get; private set; } = null!;
    public Timeframe Timeframe { get; private set; }
    public MonitorStatus Status { get; private set; }
    public StrategyPositionState PositionState { get; private set; }
    public DateTime CreateTime { get; private set; }
    public DateTime? StopTime { get; private set; }
    public DateOnly? LastEvaluatedCandleDate { get; private set; }

    private StrategyMonitor()
    {
    }

    public StrategyMonitor(
        Guid chatId,
        int tradeStrategyId,
        Guid stockId,
        string symbol,
        Timeframe timeframe,
        DateTime nowUtc)
    {
        if (chatId == Guid.Empty) throw new DomainException("Invalid chatId");
        if (tradeStrategyId <= 0) throw new DomainException("Invalid tradeStrategyId");
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

    public void Activate()
    {
        if (Status == MonitorStatus.Active)
            throw new DomainException("strategy is already monitored");

        PositionState = StrategyPositionState.OutOfMarket();
        Status = MonitorStatus.Active;
        StopTime = null;
    }

    public void Stop()
    {
        if (Status == MonitorStatus.Stopped)
            throw new DomainException("strategy is already stopped");

        Status = MonitorStatus.Stopped;
        StopTime = DateTime.UtcNow;
    }
    
    public void ApplyEvaluation(StrategyEvaluationResult evaluation)
    {
        UpdatePositionState(evaluation.PositionStateAfter);
        MarkEvaluated(evaluation.CandleDate);

        if (!evaluation.ShouldNotify)
            return;

        RaiseDomainEvent(new StrategyMonitorAlertDomainEvent(
            Id,
            ChatId,
            TradeStrategyId,
            Symbol,
            evaluation));
    }

    private void MarkEvaluated(DateOnly candleDate)
    {
        if (Status != MonitorStatus.Active)
            throw new DomainException("Cannot evaluate stopped monitor.");

        if (LastEvaluatedCandleDate is not null &&
            candleDate <= LastEvaluatedCandleDate.Value)
        {
            throw new DomainException("Candle was already evaluated.");
        }

        LastEvaluatedCandleDate = candleDate;
    }

    private void UpdatePositionState(StrategyPositionState positionState)
    {
        PositionState = positionState
                        ?? throw new DomainException("Position state is required.");
    }
}