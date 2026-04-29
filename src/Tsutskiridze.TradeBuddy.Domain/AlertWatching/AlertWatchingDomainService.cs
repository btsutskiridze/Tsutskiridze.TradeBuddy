using SharedKernel;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;

namespace Tsutskiridze.TradeBuddy.Domain.AlertWatching;

public sealed class AlertWatchingDomainService
{
    public void ActivateAlert(PriceAlert alert, Stock stock)
    {
        EnsureSameStock(alert, stock);
        alert.Activate();

        if (alert.IsActive)
        {
            stock.StartPriceMonitoring();
        }
    }

    public void DeactivateAlert(PriceAlert alert, Stock stock, bool hasOtherActiveAlertsForStock)
    {
        EnsureSameStock(alert, stock);
        alert.Deactivate();

        if (!hasOtherActiveAlertsForStock)
        {
            stock.StopPriceMonitoring();
        }
    }

    public void EnsureStockWatchState(Stock stock, bool hasAnyActiveAlertsForStock)
    {
        if (hasAnyActiveAlertsForStock)
        {
            stock.StartPriceMonitoring();
        }
        else
        {
            stock.StopPriceMonitoring();
        }
    }


    private void EnsureSameStock(PriceAlert priceAlert, Stock stock)
    {
        if (priceAlert.StockId != stock.Id)
            throw new DomainException("Price alert belongs to different stock.");
    }
    //
    //
    // private static AlertWatchingResult BuildResult(Stock stock, bool wasWatched)
    // {
    //     return (stock.IsWatched, wasWatched) switch
    //     {
    //         (true,  true)  => new AlertWatchingResult(stock.Id, stock.Symbol, StockWatchTransition.None),
    //         (true,  false) => new AlertWatchingResult(stock.Id, stock.Symbol, StockWatchTransition.BecomeWatched),
    //         (false, true)  => new AlertWatchingResult(stock.Id, stock.Symbol, StockWatchTransition.BecomeUnwatched),
    //         (false, false) => new AlertWatchingResult(stock.Id, stock.Symbol, StockWatchTransition.None)
    //     };
    // }
}