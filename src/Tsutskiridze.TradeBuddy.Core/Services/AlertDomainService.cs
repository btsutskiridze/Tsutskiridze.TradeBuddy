using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Services;

public sealed class AlertDomainService : IAlertDomainService
{
    private readonly IRepository<Stock> _stocks;
    private readonly IReadRepository<Chat> _chats;
    private readonly IRepository<PriceAlert> _alerts;

    public AlertDomainService(
        IRepository<Stock> stocks,
        IReadRepository<Chat> chats,
        IRepository<PriceAlert> alerts)
    {
        _stocks = stocks;
        _chats = chats;
        _alerts = alerts;
    }

    public async Task CreateAlertAsync(
        long telegramChatId,
        string symbol,
        string currency,
        string stockName,
        decimal price,
        PriceDirection direction,
        CancellationToken ct)
    {
        var chatId = await _chats.FirstOrDefaultAsync(new ActiveChatIdByTelegramId(telegramChatId), ct)
                     ?? throw new DomainException("Active Chat not found.");

        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(symbol), ct);

        if (stock is null)
        {
            stock = new Stock(symbol, currency, stockName);
            await _stocks.AddAsync(stock, ct);
        }

        var alert = await _alerts.FirstOrDefaultAsync(new DuplicateAlertSpec(chatId, stock.Id, direction, price), ct);

        if (alert is not null)
        {
            if (alert.IsActive)
                throw new DomainException("Duplicate alert for same stock+direction+price.");

            alert.Activate();
        }
        else
        {
            alert = new PriceAlert(Guid.NewGuid(), chatId, stock.Id, price, direction);
            await _alerts.AddAsync(alert, ct);
        }

        stock.Watch();
    }
}