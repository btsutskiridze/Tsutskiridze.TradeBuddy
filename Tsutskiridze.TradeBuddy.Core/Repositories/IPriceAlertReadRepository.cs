using Tsutskiridze.TradeBuddy.Core.Aggregates.Chats;
using Tsutskiridze.TradeBuddy.Core.Aggregates.PriceAlerts;

namespace Tsutskiridze.TradeBuddy.Core.Repositories;

public interface IPriceAlertReadRepository
{
    Task<IReadOnlyList<PriceAlert>> GetTriggeredAlerts(string symbol, decimal price, CancellationToken ct = default);
}