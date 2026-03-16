using Mediator;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications.Telegram;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Application.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Notifications.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;

public record CreateAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price) : ICommand;

public class CreateAlertHandler : ICommandHandler<CreateAlertCommand>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly ITelegramSender _sender;
    private readonly IYahooMarketDataProvider _yahoo;
    private readonly IMediator _mediator;
    private readonly IDbExceptionClassifier _excClassifier;


    public CreateAlertHandler(
        IUnitOfWork uow,
        ITelegramSender sender,
        IYahooMarketDataProvider yahoo,
        IMediator mediator,
        IRepository<Stock> stocks,
        IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts, 
        IDbExceptionClassifier excClassifier)
    {
        _uow = uow;
        _sender = sender;
        _yahoo = yahoo;
        _mediator = mediator;
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
        _excClassifier = excClassifier;
    }

    public async ValueTask<Unit> Handle(CreateAlertCommand command, CancellationToken ct)
    {
        await _sender.SendTypingAction(command.ChatId, ct);

        var stockQuote = await GetValidatedStockQuote(command.Symbol);
        var chat = await GetActiveChat(command.ChatId, ct);
        var stock = await GetOrCreateStock(command.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var existingAlert = await GetPriceAlert(chat.Id, stock.Id, command.Direction, command.Price, ct);

        if (existingAlert is not null)
        {
            existingAlert.Activate();
        }
        else
        {
            var alert = new PriceAlert(
                Guid.NewGuid(),
                chat.Id,
                stock.Id,
                command.Price,
                command.Direction);
            
            alert.Activate();

            await _alerts.AddAsync(alert, ct);
        }

        stock.Watch();

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (_excClassifier.IsUniqueConstraintViolation(ex, out var constraintName))
        {
            if (constraintName.Contains("UX_price_alerts")) 
            {
                throw new ValidationException("Alert already exists.");
            }
    
            if (constraintName.Contains("UX_stocks_symbol"))
            {
                throw new ApplicationLayerException("The system was updating stock data. Please try your command again."); 
            }
    
            throw;
        }
        
        await _mediator.Publish(
            new PriceAlertActivatedNotification(
                command.ChatId,
                command.Symbol,
                command.Direction,
                stockQuote.Currency,
                command.Price
            ),
            ct
        );

        return Unit.Value;
    }

    private async ValueTask<PriceAlert?> GetPriceAlert(Guid chatId, Guid stockId, PriceDirection direction, decimal price, CancellationToken ct)
    {
        var existingAlert = await _alerts.FirstOrDefaultAsync(
            new DuplicateAlertSpec(chatId, stockId, direction, price),
            ct
        );

        return existingAlert;
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
            throw new ResourceNotFoundException("Chat not found");

        chat.EnsureActivated();
        return chat;
    }

    private async Task<Stock> GetOrCreateStock(string symbol, string name, string currency, CancellationToken ct)
    {
        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(symbol), ct);
        if (stock is not null) return stock;

        stock = new Stock(symbol, currency, name);
        await _stocks.AddAsync(stock, ct);

        return stock;
    }
}