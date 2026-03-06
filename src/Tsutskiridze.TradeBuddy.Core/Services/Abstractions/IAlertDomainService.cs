using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Core.Services.Abstractions;

public interface IAlertDomainService
{
    Task CreateAlertAsync(
        long telegramChatId,
        string symbol,
        string currency,
        string stockName,
        decimal price,
        PriceAlertDirection direction,
        CancellationToken ct = default);
}
