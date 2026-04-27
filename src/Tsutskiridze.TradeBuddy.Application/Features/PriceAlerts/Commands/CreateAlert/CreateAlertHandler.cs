using System.Globalization;
using System.Net;
using Mediator;
using SharedKernel.Data;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Application.DTOs.Persistence;
using Tsutskiridze.TradeBuddy.Application.Enums;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.AlertWatching;
using Tsutskiridze.TradeBuddy.Domain.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;

public record CreateAlertCommand(long ChatId, string Symbol, PriceDirection Direction, decimal Price)
    : ICommand<CreateAlertResult>;

public sealed record CreateAlertResult(string Message);

public class CreateAlertHandler : ICommandHandler<CreateAlertCommand, CreateAlertResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<PriceAlert> _alerts;
    private readonly IMarketDataProvider _market;
    private readonly IDbExceptionClassifier _persistenceExceptionClassifier;
    private readonly AlertWatchingDomainService _alertWatchingSvc;


    public CreateAlertHandler(
        IUnitOfWork uow,
        IMarketDataProvider market,
        IRepository<Stock> stocks,
        IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts,
        IDbExceptionClassifier excClassifier,
        AlertWatchingDomainService alertWatchingSvc)
    {
        _uow = uow;
        _market = market;
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
        _persistenceExceptionClassifier = excClassifier;
        _alertWatchingSvc = alertWatchingSvc;
    }
    
    //todo: add resilience
    public async ValueTask<CreateAlertResult> Handle(CreateAlertCommand command, CancellationToken ct)
    {
        var chat = await GetActiveChat(command.ChatId, ct);
        var stockQuote = await GetValidatedStockQuote(command.Symbol);
        
        var stock = await GetOrCreateStock(command.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var alert = await GetOrCreateAlert(chat.Id, stock.Id, command.Direction, command.Price, ct);
        
        _alertWatchingSvc.ActivateAlert(alert, stock);

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (
            _persistenceExceptionClassifier.TryClassify(ex, out var error))
        {
            var newEx = MapPersistenceError(error, ex);
            if (newEx != ex)
                throw newEx;

            throw;
        }

        var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(c => new RegionInfo(c.Name).ISOCurrencySymbol == stockQuote.Currency);

        // todo: return result details and not the actual messages
        var currencySymbol = culture != null ? new RegionInfo(culture.Name).CurrencySymbol : stockQuote.Currency;
        var text = $"✅ Price alert set for {command.Symbol} {command.Direction} {currencySymbol}{command.Price}";

        return new CreateAlertResult(text);
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
            price,
            direction,
            DateTime.UtcNow
        );

        await _alerts.AddAsync(alert, ct);

        return alert;
    }

    private async Task<StockQuoteDto> GetValidatedStockQuote(string symbol)
    {
        return await _market.GetStockQuote(symbol)
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

    private async Task<Stock> GetOrCreateStock(string symbol, string name, string currency, CancellationToken ct)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(normalizedSymbol), ct);
        if (stock is not null)
            return stock;

        stock = new Stock(normalizedSymbol, currency, name);
        await _stocks.AddAsync(stock, ct);
        return stock;
    }

    private static Exception MapPersistenceError(
        PersistenceErrorDto error,
        Exception exception)
    {
        return error.Code switch
        {
            PersistenceErrorCode.DuplicatePriceAlert
                => new ApplicationLayerException(
                    "Alert already exists.",
                    (int)HttpStatusCode.Conflict,
                    exception),

            PersistenceErrorCode.DuplicateStockSymbol
                => new ApplicationLayerException(
                    "Stock was created by another request. Please try again.",
                    (int)HttpStatusCode.Conflict,
                    exception),

            _ => exception
        };
    }
}