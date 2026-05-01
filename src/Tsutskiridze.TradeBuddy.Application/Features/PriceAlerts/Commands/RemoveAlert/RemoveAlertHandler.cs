using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.Enums;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.RemoveAlert;

public sealed record RemoveAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price)
    : ICommand<RemoveAlertCommandResult>;

public sealed record RemoveAlertCommandResult(string Symbol, PriceDirection Direction, decimal Price);

public sealed class RemoveAlertHandler : ICommandHandler<RemoveAlertCommand, RemoveAlertCommandResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly AlertWatchingDomainService _alertWatchingSvc;

    public RemoveAlertHandler(IUnitOfWork uow, 
        IRepository<Stock> stocks,
        IRepository<PriceAlert> alerts, 
        AlertWatchingDomainService alertWatchingSvc, 
        IActiveChatProvider activeChatProvider)
    {
        _uow = uow;
        _stocks = stocks;
        _alerts = alerts;
        _alertWatchingSvc = alertWatchingSvc;
        _activeChatProvider = activeChatProvider;
    }
    
    public async ValueTask<RemoveAlertCommandResult> Handle(RemoveAlertCommand command, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(command.ChatId, ct);
        var stock = await GetStock(command.Symbol, ct);
        var alert = await GetAlert(chatId, stock.Id, command.Direction, command.Price, ct);
        var hasOtherActiveAlertsForStock =
            await _alerts.AnyAsync(new ActiveAlertsByStockIdExceptAlertSpec(stock.Id, alert.Id), ct);

        _alertWatchingSvc.DeactivateAlert(alert, stock, hasOtherActiveAlertsForStock);
        
        await _uow.SaveChangesAsync(ct);
        
        return new RemoveAlertCommandResult(stock.Symbol, alert.Trigger.Direction, alert.Trigger.Price);
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
