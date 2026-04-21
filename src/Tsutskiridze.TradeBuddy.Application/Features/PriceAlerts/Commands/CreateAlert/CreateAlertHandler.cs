using System.Globalization;
using Mediator;
using SharedKernel;
using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Common.Data;
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
    private readonly IMarketDataProvider _yahoo;
    private readonly IDbExceptionClassifier _excClassifier;


    public CreateAlertHandler(
        IUnitOfWork uow,
        IMarketDataProvider yahoo,
        IRepository<Stock> stocks,
        IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts,
        IDbExceptionClassifier excClassifier)
    {
        _uow = uow;
        _yahoo = yahoo;
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
        _excClassifier = excClassifier;
    }

    public async ValueTask<CreateAlertResult> Handle(CreateAlertCommand command, CancellationToken ct)
    {
        var chat = await GetActiveChat(command.ChatId, ct);
        var stockQuote = await GetValidatedStockQuote(command.Symbol);
        var stock = await GetOrCreateStock(command.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var existingAlert = await GetPriceAlert(chat.Id, stock.Id, command.Direction, command.Price, ct);

        if (existingAlert is null)
        {
            var alert = new PriceAlert(
                Guid.NewGuid(),
                chat.Id,
                stock.Id,
                command.Price,
                command.Direction,
                DateTime.UtcNow);

            alert.Activate();

            await _alerts.AddAsync(alert, ct);
        }
        else
            existingAlert.Activate();

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (_excClassifier.IsUniqueConstraintViolation(ex, out var constraintName))
        {
            if (constraintName.Contains("UX_price_alerts"))
            {
                throw new ValidationException("Alert already exists.");
            }

            if (constraintName.Contains("UX_stocks_symbol"))
            {
                throw new ApplicationLayerException(
                    "The system was updating stock data. Please try your command again.");
            }

            throw;
        }

        var culture = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .FirstOrDefault(c => new RegionInfo(c.Name).ISOCurrencySymbol == stockQuote.Currency);
        
        // todo: return result details and not the actual messages
        var currencySymbol = culture != null ? new RegionInfo(culture.Name).CurrencySymbol : stockQuote.Currency;
        var text = $"✅ Price alert set for {command.Symbol} {command.Direction} {currencySymbol}{command.Price}";

        return new CreateAlertResult(text);
    }

    private async ValueTask<PriceAlert?> GetPriceAlert(Guid chatId, Guid stockId, PriceDirection direction,
        decimal price, CancellationToken ct)
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
            throw new ResourceNotFoundException("Chat isn't activated.");

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