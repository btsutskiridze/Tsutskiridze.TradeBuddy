using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;

public interface IYahooStockQuoteApiProvider
{
    Task<StockQuote?> GetStockQuote(string symbol, CancellationToken ct);
}