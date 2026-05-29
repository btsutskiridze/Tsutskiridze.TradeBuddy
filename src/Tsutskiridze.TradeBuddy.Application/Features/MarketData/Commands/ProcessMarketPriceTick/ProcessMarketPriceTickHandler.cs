using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.MarketData.Commands.ProcessMarketPriceTick;

public record ProcessMarketPriceTickCommand(string Symbol, decimal Price, DateTime OccurredAtUtc) : ICommand;

public sealed class ProcessMarketPriceTickHandler : ICommandHandler<ProcessMarketPriceTickCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly AlertWatchingDomainService _alertWatchingSvc;
    private readonly ILogger<ProcessMarketPriceTickHandler> _log;

    private static readonly AlertTriggerPolicy AlertTriggerPolicy = new(
        TimeSpan.FromSeconds(10),
        5);

    public ProcessMarketPriceTickHandler(
        IUnitOfWork uow,
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts,
        ILogger<ProcessMarketPriceTickHandler> log,
        AlertWatchingDomainService alertWatchingSvc)
    {
        _uow = uow;
        _stocks = stocks;
        _alerts = alerts;
        _log = log;
        _alertWatchingSvc = alertWatchingSvc;
    }

    public async ValueTask<Unit> Handle(ProcessMarketPriceTickCommand command, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(command.Symbol), ct);

        if (stock is null)
            return Unit.Value;

        var activeAlerts = await _alerts.ListAsync(new ActiveAlertsByStockIdSpec(stock.Id), ct);

        if (activeAlerts.Count == 0)
            return Unit.Value;

        var tick = new PriceTick(command.Price, command.OccurredAtUtc);
        var triggeredCount = 0;

        foreach (var alert in activeAlerts)
        {
            ct.ThrowIfCancellationRequested();

            if (alert.ProcessMarketPrice(tick, AlertTriggerPolicy))
                triggeredCount++;
        }

        _alertWatchingSvc.EnsureStockWatchState(stock, activeAlerts.Any(x => x.IsActive));

        await _uow.SaveChangesAsync(ct);

        _log.LogInformation(
            "Processed market price update for {Symbol}. Alerts triggered: {TriggeredCount}",
            command.Symbol,
            triggeredCount);

        return Unit.Value;
    }
}