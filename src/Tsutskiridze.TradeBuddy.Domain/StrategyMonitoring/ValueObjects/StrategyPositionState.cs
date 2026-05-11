using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;

namespace Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;

public sealed record StrategyPositionState
{
    public PositionSide Side { get; init; }
    public decimal? EntryPrice { get; init; }
    public DateTime? EntryDate { get; init; }
    public decimal? LockedAtr { get; init; }
    public decimal? HighestClose { get; init; }
    public decimal? ActiveStop { get; init; }
    public bool TrailingActivated { get; init; }

    private StrategyPositionState()
    {
        // EF Core
    }

    private StrategyPositionState(
        PositionSide side,
        decimal? entryPrice,
        DateTime? entryDate,
        decimal? lockedAtr,
        decimal? highestClose,
        decimal? activeStop,
        bool trailingActivated)
    {
        if (!Enum.IsDefined(side))
            throw new DomainException("Invalid position side.");

        Side = side;
        EntryPrice = entryPrice;
        EntryDate = entryDate;
        LockedAtr = lockedAtr;
        HighestClose = highestClose;
        ActiveStop = activeStop;
        TrailingActivated = trailingActivated;

        Validate();
    }

    public static StrategyPositionState OutOfMarket()
    {
        return new StrategyPositionState(
            side: PositionSide.OutOfMarket,
            entryPrice: null,
            entryDate: null,
            lockedAtr: null,
            highestClose: null,
            activeStop: null,
            trailingActivated: false);
    }

    public StrategyPositionState EnterLong(
        decimal entryPrice,
        decimal lockedAtr,
        DateTime entryDate,
        decimal initialStop)
    {
        if (Side != PositionSide.OutOfMarket)
            throw new DomainException("Position already entered.");

        if (entryPrice <= 0)
            throw new DomainException("Entry price must be positive.");

        if (lockedAtr <= 0)
            throw new DomainException("Locked ATR must be positive.");

        if (initialStop <= 0)
            throw new DomainException("Initial stop must be positive.");

        return new StrategyPositionState(
            side: PositionSide.Long,
            entryPrice: entryPrice,
            entryDate: entryDate,
            lockedAtr: lockedAtr,
            highestClose: entryPrice,
            activeStop: initialStop,
            trailingActivated: false);
    }

    public StrategyPositionState UpdateHighestClose(decimal close)
    {
        if (Side != PositionSide.Long)
            return this;

        if (close <= 0)
            throw new DomainException("Close price must be positive.");

        if (HighestClose is not null && close <= HighestClose.Value)
            return this;

        return this with { HighestClose = close };
    }

    public StrategyPositionState ActivateTrailing()
    {
        if (Side != PositionSide.Long)
            throw new DomainException("Cannot activate trailing without long position.");

        return this with { TrailingActivated = true };
    }

    public StrategyPositionState RatchetStop(decimal newStop)
    {
        if (Side != PositionSide.Long)
            throw new DomainException("Cannot ratchet stop without long position.");

        if (newStop <= 0)
            throw new DomainException("Stop price must be positive.");

        if (ActiveStop is not null && newStop <= ActiveStop.Value)
            return this;

        return this with { ActiveStop = newStop };
    }

    public StrategyPositionState Exit()
    {
        return OutOfMarket();
    }

    private void Validate()
    {
        if (Side == PositionSide.OutOfMarket)
        {
            if (EntryPrice is not null ||
                EntryDate is not null ||
                LockedAtr is not null ||
                HighestClose is not null ||
                ActiveStop is not null ||
                TrailingActivated)
            {
                throw new DomainException("Out of market position cannot have active trade values.");
            }

            return;
        }

        if (Side == PositionSide.Long)
        {
            if (EntryPrice is null ||
                EntryDate is null ||
                LockedAtr is null ||
                HighestClose is null ||
                ActiveStop is null)
            {
                throw new DomainException("Long position must have trade values.");
            }

            return;
        }

        throw new DomainException("Unsupported position side.");
    }
}