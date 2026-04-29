using System.Collections.Immutable;
using System.Text;
using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Common.Localization;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Domain.Aggregates.Stocks.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Queries.GetMyAlerts;

public sealed record MyAlertsQuery(long ChatId) : IQuery<MyAlertsResult>;

public sealed record MyAlertsResult(string Message);

public sealed class GetMyAlertsHandler : IQueryHandler<MyAlertsQuery, MyAlertsResult>
{
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IReadRepository<Stock> _stocks;
    private readonly IReadRepository<PriceAlert> _alerts;
    private readonly ICurrencySymbolProvider _currency;

    public GetMyAlertsHandler(
        IReadRepository<Stock> stocks,
        IReadRepository<PriceAlert> alerts,
        ICurrencySymbolProvider currency, 
        IActiveChatProvider activeChatProvider)
    {
        _stocks = stocks;
        _alerts = alerts;
        _currency = currency;
        _activeChatProvider = activeChatProvider;
    }

    public async ValueTask<MyAlertsResult> Handle(MyAlertsQuery query, CancellationToken ct)
    {
        var chatId = await _activeChatProvider.GetIdAsync(query.ChatId, ct);
        var alerts = await _alerts.ListAsync(new ActiveAlertsByChatIdSpec(chatId), ct);
        if (alerts.Count == 0) throw new ApplicationLayerException("You have no alerts set.");

        var stockIds = alerts.Select(x => x.StockId).ToList();
        var stocks = (await _stocks.ListAsync(new StocksByIdsSpec(stockIds), ct))
            .ToImmutableDictionary(x => x.Id);

        var alertsMessage = alerts
            .Select(x =>
            {
                var stock = stocks[x.StockId];
                return $"*{stock.Symbol}* {x.Direction} {_currency.GetSymbol(stock.Currency)}{x.Price}";
            })
            .ToList();

        var text = new StringBuilder()
            .AppendLine("🔔 *Your Active Alerts* 🔔")
            .AppendLine()
            .AppendJoin("\n", alertsMessage
                .Select((a, i) => $"{i + 1}. {a}"))
            .ToString();
        
        return new MyAlertsResult(text);
    }
}