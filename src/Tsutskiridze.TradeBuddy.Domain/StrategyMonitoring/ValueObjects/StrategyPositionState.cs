using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;

public record StrategyPositionState : ValueObject
{
    public PositionSide Side { get; private set; }
    public decimal? EntryPrice { get; private set; }
    public DateTime? EntryDate { get; private set; }
    public decimal? LockedAtr { get; private set; }
    public decimal? HighestClose { get; private set; }
    public decimal? ActiveStop { get; private set; }
    public bool TrailingActivated { get; private set; }

    private StrategyPositionState()
    {
    }

    private StrategyPositionState(PositionSide side)
    {
        if (!Enum.IsDefined(side))
            throw new DomainException("Invalid position side.");

        Side = side;
    }

    public static StrategyPositionState OutOfMarket()
    {
        return new StrategyPositionState(PositionSide.OutOfMarket);
    }

    public void EnterLong(
        decimal entryPrice,
        decimal lockedAtr,
        DateTime entryDate,
        decimal initialStop
    )
    {
        if (Side != PositionSide.OutOfMarket)
            throw new DomainException("Position already entered.");

        Side = PositionSide.Long;
        EntryPrice = entryPrice;
        LockedAtr = lockedAtr;
        EntryDate = entryDate;
        ActiveStop = initialStop;
        HighestClose = entryPrice;
        TrailingActivated = false;
    }

    public void UpdateHighestClose(decimal close)
    {
        if (Side != PositionSide.Long)
            return;

        if (HighestClose is null || close > HighestClose.Value)
            HighestClose = close;
    }

    public void ActivateTrailing()
    {
        if (Side != PositionSide.Long)
            throw new DomainException("Cannot activate trailing without long position.");

        TrailingActivated = true;
    }

    public void RatchetTop(decimal newStop)
    {
        if (Side != PositionSide.Long)
            throw new DomainException("Cannot ratchet top without long position.");

        if (ActiveStop is null || newStop > ActiveStop.Value)
            ActiveStop = newStop;
    }
    
    public void Exit()
    {
        Side = PositionSide.OutOfMarket;
        EntryPrice = null;
        EntryDate = null;
        LockedAtr = null;
        HighestClose = null;
        ActiveStop = null;
        TrailingActivated = false;
    }
}