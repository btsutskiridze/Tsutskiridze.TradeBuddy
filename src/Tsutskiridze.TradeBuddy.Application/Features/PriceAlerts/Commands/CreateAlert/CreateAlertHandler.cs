using System.Data;
using Mediator;
using SharedKernel.Data;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Services;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;

public record CreateAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price)
    : ICommand<CreateAlertResult>;

public sealed record CreateAlertResult(string Symbol, string CurrencyCode, PriceDirection Direction, decimal Price);

public class CreateAlertHandler : ICommandHandler<CreateAlertCommand, CreateAlertResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IStockService _stocks;
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly IMarketDataProvider _market;
    private readonly AlertWatchingDomainService _alertWatchingSvc;


    public CreateAlertHandler(
        IUnitOfWork uow,
        IStockService stocks,
        IMarketDataProvider market,
        IActiveChatProvider activeChatProvider,
        IRepository<PriceAlert> alerts,
        AlertWatchingDomainService alertWatchingSvc)
    {
        _uow = uow;
        _market = market;
        _activeChatProvider = activeChatProvider;
        _alerts = alerts;
        _alertWatchingSvc = alertWatchingSvc;
        _stocks = stocks;
    }

    //todo: add resilience
    public async ValueTask<CreateAlertResult> Handle(CreateAlertCommand command, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(command.ChatId, ct);
        var stockQuote = await GetValidatedStockQuote(command.Symbol);

        await using var tx = await _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        var stock = await _stocks.GetOrCreateStock(command.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var alert = await GetOrCreateAlert(chatId, stock.Id, command.Direction, command.Price, ct);

        _alertWatchingSvc.ActivateAlert(alert, stock);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new CreateAlertResult(stock.Symbol, stock.Currency, alert.Trigger.Direction, alert.Trigger.Price);
    }

    private async Task<PriceAlert> GetOrCreateAlert(Guid chatId, Guid stockId, PriceDirection direction,
        decimal price, CancellationToken ct)
    {
        var alert = await _alerts.FirstOrDefaultAsync(
            new DuplicateAlertSpec(chatId, stockId, direction, price),
            ct
        );

        if (alert != null)
        {
            return alert;
        }

        alert = new PriceAlert(
            Guid.NewGuid(),
            chatId,
            stockId,
            AlertTrigger.Create(direction, price),
            DateTime.UtcNow
        );

        await _alerts.AddAsync(alert, ct);

        return alert;
    }

    private async Task<StockQuote> GetValidatedStockQuote(string symbol)
    {
        return await _market.GetStockQuote(symbol)
               ?? throw new ValidationException("Stock symbol not found");
    }

    // persistence exception mapping is being moved out of handlers
}