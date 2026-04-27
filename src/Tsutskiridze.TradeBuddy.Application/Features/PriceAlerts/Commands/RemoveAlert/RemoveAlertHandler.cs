using System.Data;
using Mediator;
using SharedKernel;
using SharedKernel.Data;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Common.Localization;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.RemoveAlert;

public sealed record RemoveAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price)
    : ICommand<RemoveAlertCommandResult>;

public sealed record RemoveAlertCommandResult(string Message);

public sealed class RemoveAlertHandler : ICommandHandler<RemoveAlertCommand, RemoveAlertCommandResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly AlertWatchingDomainService _alertWatchingSvc;

    public RemoveAlertHandler(IUnitOfWork uow, IRepository<Stock> stocks, IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts, AlertWatchingDomainService alertWatchingSvc)
    {
        _uow = uow;
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
        _alertWatchingSvc = alertWatchingSvc;
    }


    /*
     *todo:
     * Remove the quote lookup from the delete path and use command.Price
     * or the stored alert value in the response.
     */
    public async ValueTask<RemoveAlertCommandResult> Handle(RemoveAlertCommand command, CancellationToken ct)
    {
        var chat = await GetActiveChat(command.ChatId, ct);
        var stock = await GetStock(command.Symbol, ct);
        var alert = await GetAlert(chat.Id, stock.Id, command.Direction, command.Price, ct);
        var hasOtherActiveAlertsForStock =
            await _alerts.AnyAsync(new ActiveAlertsByStockIdExceptAlertSpec(stock.Id, alert.Id), ct);

        _alertWatchingSvc.DeactivateAlert(alert, stock, hasOtherActiveAlertsForStock);
        
        await _uow.SaveChangesAsync(ct);
        
        return new RemoveAlertCommandResult(
            $"Alert for *{command.Symbol}* with direction *{command.Direction.ToString().ToLower()}* and price *{command.Price}* has been removed."
        );
    }

    private async Task<Chat> GetActiveChat(long chatId, CancellationToken ct)
    {
        var chat = await _chats.FirstOrDefaultAsync(new ChatByTelegramIdSpec(chatId), ct);
        if (chat is null)
            throw new ResourceNotFoundException("Chat isn't activated.");

        chat.EnsureActivated();
        return chat;
    }

    private async Task<Stock> GetStock(string symbol, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(symbol), ct);
        return stock ?? throw new ResourceNotFoundException("Stock not found.");
    }

    private async ValueTask<PriceAlert> GetAlert(Guid chatId, Guid stockId, PriceDirection direction,
        decimal price, CancellationToken ct)
    {
        var existingAlert = await _alerts.FirstOrDefaultAsync(
            new DuplicateAlertSpec(chatId, stockId, direction, price),
            ct
        );

        return existingAlert ?? throw new ResourceNotFoundException("Alert not found.");
    }
}