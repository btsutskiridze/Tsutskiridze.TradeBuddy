using System.Text.Json;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing;

public class YahooQuotePageParser : IYahooQuotePageParser
{
    public YahooQuotePageParser(IYahooJsonNavigator jsonNavigator)
    {
        _jsonNavigator = jsonNavigator;
    }

    public bool StockSymbolExists(YahooPageContext pageContext) =>
        _jsonNavigator.FindFirstQuoteObject(pageContext.PayloadRoots, pageContext.Symbol) is not null ||
        YahooHtmlValueReader.IsQuoteTitleMatch(pageContext.Document, pageContext.Symbol, out _);

    public StockQuoteDto? Parse(YahooPageContext pageContext)
    {
        var quote = new StockQuoteDto
        {
            Symbol = pageContext.Symbol
        };

        var quoteMatchedByJson = ApplyQuoteFromJson(pageContext.PayloadRoots, pageContext.Symbol, quote);

        if (!quoteMatchedByJson)
        {
            if (!YahooHtmlValueReader.IsQuoteTitleMatch(pageContext.Document, pageContext.Symbol, out var titleText))
                return null;

            if (string.IsNullOrWhiteSpace(quote.Name))
                quote.Name = titleText;
        }

        ApplyQuoteFromHtmlFallback(pageContext.Document, quote);

        return quote;
    }

    private bool ApplyQuoteFromJson(IReadOnlyList<JsonElement> payloadRoots, string symbol, StockQuoteDto quote)
    {
        JsonElement? quoteJson = null;
        JsonElement? summaryDetailJson = null;
        JsonElement? financialDataJson = null;
        JsonElement? defaultKeyStatisticsJson = null;

        foreach (var root in payloadRoots)
        {
            quoteJson ??= _jsonNavigator.FindQuoteObjectBySymbol(root, symbol);
            summaryDetailJson ??= _jsonNavigator.FindObjectByPropertyName(root, "summaryDetail");
            financialDataJson ??= _jsonNavigator.FindObjectByPropertyName(root, "financialData");
            defaultKeyStatisticsJson ??= _jsonNavigator.FindObjectByPropertyName(root, "defaultKeyStatistics");

            if (quoteJson is not null && summaryDetailJson is not null && financialDataJson is not null && defaultKeyStatisticsJson is not null)
                break;
        }

        var earningsDate = _jsonNavigator.FindFirstEarningsDate(payloadRoots);

        if (quoteJson is not null)
        {
            var data = quoteJson.Value;
            var longName = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "longName"));
            var shortName = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "shortName"));

            if (!string.IsNullOrWhiteSpace(longName))
                quote.Name = $"{longName} ({symbol})";
            else if (!string.IsNullOrWhiteSpace(shortName))
                quote.Name = $"{shortName} ({symbol})";

            quote.Price = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketPrice"));
            quote.Change = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketChange"));
            quote.ChangesPercentage = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketChangePercent"));
            quote.Exchange = YahooValueFormatter.PreferExisting(
                quote.Exchange,
                YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "fullExchangeName")),
                YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "exchange")));
            quote.Currency = YahooValueFormatter.PreferExisting(quote.Currency, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "currency")));
            quote.Timestamp = YahooValueFormatter.PreferExisting(quote.Timestamp, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketTime")));
            quote.PreviousClose = YahooValueFormatter.PreferExisting(quote.PreviousClose, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketPreviousClose")));
            quote.Open = YahooValueFormatter.PreferExisting(quote.Open, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketOpen")));
            quote.MarketCap = YahooValueFormatter.PreferExisting(quote.MarketCap, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "marketCap")));
            quote.Volume = YahooValueFormatter.PreferExisting(quote.Volume, YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketVolume"))));
            quote.DayLow = YahooValueFormatter.PreferExisting(quote.DayLow, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketDayLow")));
            quote.DayHigh = YahooValueFormatter.PreferExisting(quote.DayHigh, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketDayHigh")));
            quote.YearLow = YahooValueFormatter.PreferExisting(quote.YearLow, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "fiftyTwoWeekLow")));
            quote.YearHigh = YahooValueFormatter.PreferExisting(quote.YearHigh, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "fiftyTwoWeekHigh")));
        }

        if (summaryDetailJson is not null)
        {
            var data = summaryDetailJson.Value;
            quote.PreviousClose = YahooValueFormatter.PreferExisting(quote.PreviousClose, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketPreviousClose")));
            quote.Open = YahooValueFormatter.PreferExisting(quote.Open, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketOpen")));
            quote.MarketCap = YahooValueFormatter.PreferExisting(quote.MarketCap, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "marketCap")));
            quote.Volume = YahooValueFormatter.PreferExisting(quote.Volume, YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketVolume"))));
            quote.AvgVolume = YahooValueFormatter.PreferExisting(quote.AvgVolume, YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "averageVolume"))));
            quote.DayLow = YahooValueFormatter.PreferExisting(quote.DayLow, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketDayLow")));
            quote.DayHigh = YahooValueFormatter.PreferExisting(quote.DayHigh, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "regularMarketDayHigh")));
            quote.YearLow = YahooValueFormatter.PreferExisting(quote.YearLow, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "fiftyTwoWeekLow")));
            quote.YearHigh = YahooValueFormatter.PreferExisting(quote.YearHigh, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "fiftyTwoWeekHigh")));
        }

        if (financialDataJson is not null)
        {
            var data = financialDataJson.Value;
            quote.Price = YahooValueFormatter.PreferExisting(quote.Price, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "currentPrice")));
            quote.Eps = YahooValueFormatter.PreferExisting(quote.Eps, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "trailingEps")));
            quote.Currency = YahooValueFormatter.PreferExisting(quote.Currency, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetString(data, "financialCurrency")));
        }

        if (defaultKeyStatisticsJson is not null)
        {
            var data = defaultKeyStatisticsJson.Value;
            quote.Eps = YahooValueFormatter.PreferExisting(quote.Eps, YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "trailingEps")));

            if (string.IsNullOrWhiteSpace(quote.Pe))
            {
                var peValue = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "trailingPE"));
                quote.Pe = string.IsNullOrWhiteSpace(peValue) || peValue == "--" ? null : peValue;
            }
        }

        quote.EarningsAnnouncement = YahooValueFormatter.PreferExisting(quote.EarningsAnnouncement, earningsDate);

        return quoteJson is not null;
    }

    private static void ApplyQuoteFromHtmlFallback(HtmlAgilityPack.HtmlDocument document, StockQuoteDto quote)
    {
        if (string.IsNullOrWhiteSpace(quote.Name) && YahooHtmlValueReader.IsQuoteTitleMatch(document, quote.Symbol, out var titleText))
            quote.Name = titleText;

        quote.Price = YahooValueFormatter.PreferExisting(quote.Price, YahooHtmlValueReader.GetNodeText(document, "//span[@data-testid='qsp-price']"));
        quote.Change = YahooValueFormatter.PreferExisting(quote.Change, YahooHtmlValueReader.GetNodeText(document, "//span[@data-testid='qsp-price-change']"));
        quote.ChangesPercentage = YahooValueFormatter.PreferExisting(
            quote.ChangesPercentage,
            YahooHtmlValueReader.GetNodeText(document, "//span[@data-testid='qsp-price-change-percent']").Replace("(", string.Empty).Replace(")", string.Empty));
        quote.Timestamp = YahooValueFormatter.PreferExisting(quote.Timestamp, YahooHtmlValueReader.GetNodeText(document, "//div[@slot='marketTimeNotice']"));
        quote.PreviousClose = YahooValueFormatter.PreferExisting(quote.PreviousClose, YahooHtmlValueReader.GetQuoteSummaryValue(document, "Previous Close"));
        quote.Open = YahooValueFormatter.PreferExisting(quote.Open, YahooHtmlValueReader.GetQuoteSummaryValue(document, "Open"));
        quote.MarketCap = YahooValueFormatter.PreferExisting(quote.MarketCap, YahooHtmlValueReader.GetQuoteSummaryValue(document, "Market Cap (intraday)"));
        quote.Volume = YahooValueFormatter.PreferExisting(quote.Volume, YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.GetQuoteSummaryValue(document, "Volume")));
        quote.AvgVolume = YahooValueFormatter.PreferExisting(quote.AvgVolume, YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.GetQuoteSummaryValue(document, "Avg. Volume")));
        quote.Eps = YahooValueFormatter.PreferExisting(quote.Eps, YahooHtmlValueReader.GetQuoteSummaryValue(document, "EPS (TTM)"));

        if (string.IsNullOrWhiteSpace(quote.Pe))
        {
            var peValue = YahooHtmlValueReader.GetQuoteSummaryValue(document, "PE Ratio (TTM)");
            quote.Pe = string.IsNullOrWhiteSpace(peValue) || peValue == "--" ? null : peValue;
        }

        if (string.IsNullOrWhiteSpace(quote.DayLow) || string.IsNullOrWhiteSpace(quote.DayHigh))
        {
            var (low, high) = YahooValueFormatter.SplitRange(YahooHtmlValueReader.GetNodeValueOrText(document, "//fin-streamer[@data-field='regularMarketDayRange']"));
            quote.DayLow = YahooValueFormatter.PreferExisting(quote.DayLow, low);
            quote.DayHigh = YahooValueFormatter.PreferExisting(quote.DayHigh, high);
        }

        if (string.IsNullOrWhiteSpace(quote.YearLow) || string.IsNullOrWhiteSpace(quote.YearHigh))
        {
            var (low, high) = YahooValueFormatter.SplitRange(YahooHtmlValueReader.GetNodeValueOrText(document, "//fin-streamer[@data-field='fiftyTwoWeekRange']"));
            quote.YearLow = YahooValueFormatter.PreferExisting(quote.YearLow, low);
            quote.YearHigh = YahooValueFormatter.PreferExisting(quote.YearHigh, high);
        }

        quote.EarningsAnnouncement = YahooValueFormatter.PreferExisting(quote.EarningsAnnouncement, YahooHtmlValueReader.GetEarningsAnnouncementFromHtml(document));

        if (string.IsNullOrWhiteSpace(quote.Exchange) || string.IsNullOrWhiteSpace(quote.Currency))
        {
            var exchangeText = YahooHtmlValueReader.GetNodeText(document, "//span[contains(@class, 'exchange')]");
            if (!string.IsNullOrWhiteSpace(exchangeText))
            {
                var parts = exchangeText.Split('•', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                    quote.Exchange = YahooValueFormatter.PreferExisting(quote.Exchange, parts[0]);

                if (parts.Length > 1)
                    quote.Currency = YahooValueFormatter.PreferExisting(quote.Currency, parts[^1]);
            }
        }
    }

    private readonly IYahooJsonNavigator _jsonNavigator;
}

