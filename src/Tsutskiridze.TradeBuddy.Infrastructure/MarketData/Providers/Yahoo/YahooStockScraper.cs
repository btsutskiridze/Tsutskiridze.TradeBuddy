using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions.Parsing;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo;

public class YahooStockScraper : IYahooMarketDataProvider
{
    private readonly IYahooPageLoader _pageLoader;
    private readonly IYahooQuotePageParser _quotePageParser;
    private readonly IYahooHistoryPageParser _historyPageParser;
    private readonly IYahooKeyStatisticsPageParser _keyStatisticsPageParser;
    private readonly IYahooFinancialsPageParser _financialsPageParser;
    
    public YahooStockScraper(
        IYahooPageLoader pageLoader,
        IYahooQuotePageParser quotePageParser,
        IYahooHistoryPageParser historyPageParser,
        IYahooKeyStatisticsPageParser keyStatisticsPageParser,
        IYahooFinancialsPageParser financialsPageParser)
    {
        _pageLoader = pageLoader;
        _quotePageParser = quotePageParser;
        _historyPageParser = historyPageParser;
        _keyStatisticsPageParser = keyStatisticsPageParser;
        _financialsPageParser = financialsPageParser;
    }

    public async Task<bool> StockSymbolExists(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return false;

        var pageContext = await _pageLoader.LoadQuotePageAsync(normalizedSymbol);
        if (pageContext is null)
            return false;

        return _quotePageParser.StockSymbolExists(pageContext);
    }

    public async Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return new List<StockDayPriceDto>();

        var pageContext = await _pageLoader.LoadHistoryPageAsync(normalizedSymbol);
        if (pageContext is null)
            return new List<StockDayPriceDto>();

        var prices = _historyPageParser.Parse(pageContext);

        if (days is > 0)
            prices = prices.Take(days.Value).ToList();

        return prices;
    }

    public async Task<StockOverviewDto?> GetStockOverview(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var pageContext = await _pageLoader.LoadKeyStatisticsPageAsync(normalizedSymbol);
        if (pageContext is null)
            return null;

        return _keyStatisticsPageParser.Parse(pageContext);
    }

    public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var pageContext = await _pageLoader.LoadFinancialsPageAsync(normalizedSymbol);
        if (pageContext is null)
            return null;

        return _financialsPageParser.Parse(pageContext);
    }

    public async Task<StockQuoteDto?> GetStockQuote(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var pageContext = await _pageLoader.LoadQuotePageAsync(normalizedSymbol);
        if (pageContext is null)
            return null;

        return _quotePageParser.Parse(pageContext);
    }

    private static bool TryNormalizeSymbol(string? symbol, out string normalizedSymbol)
    {
        normalizedSymbol = string.Empty;

        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        normalizedSymbol = symbol.Trim().ToUpperInvariant();
        return true;
    }
}

