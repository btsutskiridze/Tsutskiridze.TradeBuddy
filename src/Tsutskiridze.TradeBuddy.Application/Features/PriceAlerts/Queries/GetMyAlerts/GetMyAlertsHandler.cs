using System.Collections.Immutable;
using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.PriceAlerts.Enums;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Queries.GetMyAlerts;

public sealed record MyAlertsQuery(long ChatId) : IQuery<MyAlertsResult>;

public sealed record MyAlertsResult(IReadOnlyList<MyAlertItem> Alerts);

public sealed record MyAlertItem(string Symbol, PriceDirection Direction, string CurrencyCode, decimal Price);

public sealed class GetMyAlertsHandler : IQueryHandler<MyAlertsQuery, MyAlertsResult>
{
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IReadRepository<Stock> _stocks;
    private readonly IReadRepository<PriceAlert> _alerts;

    public GetMyAlertsHandler(
        IReadRepository<Stock> stocks,
        IReadRepository<PriceAlert> alerts,
        IActiveChatProvider activeChatProvider)
    {
        _stocks = stocks;
        _alerts = alerts;
        _activeChatProvider = activeChatProvider;
    }

    public async ValueTask<MyAlertsResult> Handle(MyAlertsQuery query, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(query.ChatId, ct);
        var alerts = await _alerts.ListAsync(new ActiveAlertsByChatIdSpec(chatId), ct);
        if (alerts.Count == 0) return new MyAlertsResult([]);

        var stockIds = alerts.Select(x => x.StockId).ToList();
        var stocks = (await _stocks.ListAsync(new StocksByIdsSpec(stockIds), ct))
            .ToImmutableDictionary(x => x.Id);

        var items = alerts
            .Select(x =>
            {
                var stock = stocks[x.StockId];
                return new MyAlertItem(stock.Symbol, x.Trigger.Direction, stock.Currency, x.Trigger.Price);
            })
            .ToList();

        return new MyAlertsResult(items);
    }
}