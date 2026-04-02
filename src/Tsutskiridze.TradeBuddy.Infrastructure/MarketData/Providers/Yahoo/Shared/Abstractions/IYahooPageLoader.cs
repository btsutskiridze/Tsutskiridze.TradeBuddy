namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;

public interface IYahooPageLoader
{
    Task<YahooPageContext?> LoadQuotePageAsync(string symbol);
    Task<YahooPageContext?> LoadHistoryPageAsync(string symbol);
    Task<YahooPageContext?> LoadKeyStatisticsPageAsync(string symbol);
    Task<YahooPageContext?> LoadFinancialsPageAsync(string symbol);
    Task<YahooPageContext?> LoadNewsPageAsync(string symbol);
}

