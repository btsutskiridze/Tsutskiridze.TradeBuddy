using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.MarketData;

public class MarketDataProvider : IMarketDataProvider
{
    private readonly IYahooPageLoader _pageLoader;
    private readonly IYahooQuotePageParser _quotePageParser;
    private readonly IYahooHistoryPageParser _historyPageParser;
    private readonly IYahooKeyStatisticsPageParser _keyStatisticsPageParser;
    private readonly IYahooFinancialsPageParser _financialsPageParser;
    private readonly IYahooHistoryApiProvider _historyApiProvider;
    private readonly IYahooStockQuoteApiProvider _stockQuoteApiProvider;
    
    public MarketDataProvider(
        IYahooPageLoader pageLoader,
        IYahooQuotePageParser quotePageParser,
        IYahooHistoryPageParser historyPageParser,
        IYahooKeyStatisticsPageParser keyStatisticsPageParser,
        IYahooFinancialsPageParser financialsPageParser, 
        IYahooHistoryApiProvider historyApiProvider, 
        IYahooStockQuoteApiProvider stockQuoteApiProvider)
    {
        _pageLoader = pageLoader;
        _quotePageParser = quotePageParser;
        _historyPageParser = historyPageParser;
        _keyStatisticsPageParser = keyStatisticsPageParser;
        _financialsPageParser = financialsPageParser;
        _historyApiProvider = historyApiProvider;
        _stockQuoteApiProvider = stockQuoteApiProvider;
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

    public async Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return new List<StockDayPrice>();

        var pageContext = await _pageLoader.LoadHistoryPageAsync(normalizedSymbol);
        if (pageContext is null)
            return new List<StockDayPrice>();

        var prices = _historyPageParser.Parse(pageContext);

        if (days is > 0)
            prices = prices.Take(days.Value).ToList();

        return prices;
    }

    public async Task<StockOverview?> GetStockOverview(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var pageContext = await _pageLoader.LoadKeyStatisticsPageAsync(normalizedSymbol);
        if (pageContext is null)
            return null;

        return _keyStatisticsPageParser.Parse(pageContext);
    }

    public async Task<AnnualReport?> GetStockLastAnnualReport(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var pageContext = await _pageLoader.LoadFinancialsPageAsync(normalizedSymbol);
        if (pageContext is null)
            return null;

        return _financialsPageParser.Parse(pageContext);
    }

    public async Task<StockQuote?> GetStockQuote(string symbol, CancellationToken ct)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var apiQuote = await _stockQuoteApiProvider.GetStockQuote(
            normalizedSymbol,
            ct);

        if (apiQuote is not null)
            return apiQuote;

        var pageContext = await _pageLoader.LoadQuotePageAsync(normalizedSymbol);

        if (pageContext is null)
            return null;

        return _quotePageParser.Parse(pageContext);
    }

    public async Task<IReadOnlyList<MarketCandle>> GetDailyCandles(string symbol, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var result = await _historyApiProvider.GetDailyCandles(symbol, from, to, ct);

        return result;
    }
    
    public async Task<MarketHistoryDateRange> GetClosedDailyDateRange(
        string symbol,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(symbol))
            throw new ArgumentException("Invalid symbol provided");
        
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        
        return await _historyApiProvider.GetClosedDailyDateRange(
            normalizedSymbol,
            DateTimeOffset.UtcNow,
            ct);
    }

    private static bool TryNormalizeSymbol(string symbol, out string normalizedSymbol)
    {
        normalizedSymbol = string.Empty;

        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        normalizedSymbol = symbol.Trim().ToUpperInvariant();
        return true;
    }
}

