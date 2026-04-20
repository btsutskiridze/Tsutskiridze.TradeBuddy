using Mediator;
using SharedKernel;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

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
    private readonly IMarketDataProvider _yahoo;

    public RemoveAlertHandler(IUnitOfWork uow, IRepository<Stock> stocks, IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts, IMarketDataProvider yahoo)
    {
        _uow = uow;
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
        _yahoo = yahoo;
    }

    public async ValueTask<RemoveAlertCommandResult> Handle(RemoveAlertCommand command, CancellationToken ct)
    {
        var chat = await GetActiveChat(command.ChatId, ct);
        var stockQuote = await GetValidatedStockQuote(command.Symbol);
        var stock = await GetStock(command.Symbol, ct);
        var alert = await GetPriceAlert(chat.Id, stock.Id, command.Direction, command.Price, ct);

        alert.Deactivate();
        await _uow.SaveChangesAsync(ct);

        return new RemoveAlertCommandResult(
            $"Alert for *{command.Symbol}* with direction *{command.Direction.ToString().ToLower()}* and price *{stockQuote.Price}* has been removed."
        );
    }

    private async Task<StockQuoteDto> GetValidatedStockQuote(string symbol)
    {
        return await _yahoo.GetStockQuote(symbol)
               ?? throw new ValidationException("Stock symbol not found");
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

    private async ValueTask<PriceAlert> GetPriceAlert(Guid chatId, Guid stockId, PriceDirection direction,
        decimal price, CancellationToken ct)
    {
        var existingAlert = await _alerts.FirstOrDefaultAsync(
            new DuplicateAlertSpec(chatId, stockId, direction, price),
            ct
        );

        return existingAlert ?? throw new ResourceNotFoundException("Alert not found.");
    }
}