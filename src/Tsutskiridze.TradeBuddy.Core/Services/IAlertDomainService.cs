using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Services;

public interface IAlertDomainService
{
    Task CreateAlertAsync(
        long telegramChatId,
        string symbol,
        string currency,
        string stockName,
        decimal price,
        PriceDirection direction,
        CancellationToken ct = default);
}
