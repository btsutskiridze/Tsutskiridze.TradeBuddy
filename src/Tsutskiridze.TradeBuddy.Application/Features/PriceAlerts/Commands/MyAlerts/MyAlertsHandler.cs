using System.Collections.Immutable;
using System.Text;
using Mediator;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Utilities;
using Tsutskiridze.TradeBuddy.Application.Exceptions;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts.Specifications;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Specifications;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.MyAlerts;

public sealed record MyAlertsCommand(long ChatId) : ICommand<MyAlertsResult>;

public sealed record MyAlertsResult(string Message);

public sealed class MyAlertsHandler : ICommandHandler<MyAlertsCommand, MyAlertsResult>
{
    private readonly IReadRepository<Chat> _chats;
    private readonly IReadRepository<Stock> _stocks;
    private readonly IReadRepository<PriceAlert> _alerts;
    private readonly ICurrencySymbolProvider _currency;

    public MyAlertsHandler(
        IReadRepository<Chat> chats,
        IReadRepository<Stock> stocks,
        IReadRepository<PriceAlert> alerts,
        ICurrencySymbolProvider currency)
    {
        _chats = chats;
        _stocks = stocks;
        _alerts = alerts;
        _currency = currency;
    }

    public async ValueTask<MyAlertsResult> Handle(MyAlertsCommand command, CancellationToken ct)
    {
        var chatId = await _chats.FirstOrDefaultAsync(new ActiveChatIdByTelegramId(command.ChatId), ct)
                     ?? throw new ResourceNotFoundException("Chat isn't Activated.");

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