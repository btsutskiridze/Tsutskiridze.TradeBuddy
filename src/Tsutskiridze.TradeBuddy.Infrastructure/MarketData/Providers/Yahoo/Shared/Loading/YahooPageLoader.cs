using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Loading;

public class YahooPageLoader : IYahooPageLoader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YahooPageLoader> _logger;
    private readonly IYahooCookieBypassService _yahooCookieBypassService;
    private readonly IYahooPayloadExtractor _payloadExtractor;

    public YahooPageLoader(
        HttpClient httpClient, 
        ILogger<YahooPageLoader> logger,
        IYahooCookieBypassService yahooCookieBypassService, 
        IYahooPayloadExtractor payloadExtractor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _yahooCookieBypassService = yahooCookieBypassService;
        _payloadExtractor = payloadExtractor;
    }

    public Task<YahooPageContext?> LoadQuotePageAsync(string symbol) =>
        LoadPageAsync($"quote/{Uri.EscapeDataString(symbol)}", symbol, "quote");

    public Task<YahooPageContext?> LoadHistoryPageAsync(string symbol) =>
        LoadPageAsync($"quote/{Uri.EscapeDataString(symbol)}/history", symbol, "history");

    public Task<YahooPageContext?> LoadKeyStatisticsPageAsync(string symbol) =>
        LoadPageAsync($"quote/{Uri.EscapeDataString(symbol)}/key-statistics", symbol, "key statistics");

    public Task<YahooPageContext?> LoadFinancialsPageAsync(string symbol) =>
        LoadPageAsync($"quote/{Uri.EscapeDataString(symbol)}/financials", symbol, "financials");

    public Task<YahooPageContext?> LoadNewsPageAsync(string symbol) =>
        LoadPageAsync($"quote/{Uri.EscapeDataString(symbol)}/news", symbol, "news");

    private async Task<YahooPageContext?> LoadPageAsync(string relativePath, string symbol, string pageName)
    {
        try
        {
            var document = await GetHtmlDocumentAsync(relativePath);
            var payloadRoots = _payloadExtractor.ExtractPayloadRoots(document);
            return new YahooPageContext(symbol, document, payloadRoots);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Yahoo {PageName} page for symbol {Symbol}", pageName, symbol);
            return null;
        }
    }

    protected async Task<HtmlDocument> GetHtmlDocumentAsync(string relativeUrl)
    {
        string url = $"{relativeUrl}?lang=en-US&region=US";
        string html = await _yahooCookieBypassService.GetHtmlContentWithCookieBypass(_httpClient, url, _logger);
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }
}