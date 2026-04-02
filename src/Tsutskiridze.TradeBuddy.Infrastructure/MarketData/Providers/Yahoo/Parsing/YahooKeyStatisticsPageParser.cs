using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions.Parsing;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Helpers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Parsing;

public class YahooKeyStatisticsPageParser : IYahooKeyStatisticsPageParser
{
    public YahooKeyStatisticsPageParser(IYahooJsonNavigator jsonNavigator)
    {
        _jsonNavigator = jsonNavigator;
    }

    public StockOverviewDto Parse(YahooPageContext pageContext)
    {
        var overview = new StockOverviewDto();
        ApplyOverviewFromJson(pageContext.PayloadRoots, overview);
        ApplyOverviewFromHtmlFallback(pageContext.Document, overview);
        return overview;
    }

    private void ApplyOverviewFromJson(IReadOnlyList<System.Text.Json.JsonElement> payloadRoots, StockOverviewDto overview)
    {
        var defaultKeyStatistics = _jsonNavigator.FindFirstObject(payloadRoots, "defaultKeyStatistics");
        var financialData = _jsonNavigator.FindFirstObject(payloadRoots, "financialData");

        if (financialData is not null)
        {
            var data = financialData.Value;
            overview.ReturnOnEquityTTM = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "returnOnEquity"));
            overview.QuarterlyRevenueGrowthYOY = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "revenueGrowth"));
        }

        if (defaultKeyStatistics is not null)
        {
            var data = defaultKeyStatistics.Value;
            overview.PriceToSalesRatioTTM = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "priceToSalesTrailing12Months"));
            overview.PriceAvg50 = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "fiftyDayAverage"));
            overview.PriceAvg200 = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "twoHundredDayAverage"));
            overview.SharesOutstanding = YahooHtmlValueReader.Clean(_jsonNavigator.TryGetFmtOrRawString(data, "sharesOutstanding"));
        }
    }

    private static void ApplyOverviewFromHtmlFallback(HtmlAgilityPack.HtmlDocument document, StockOverviewDto overview)
    {
        overview.ReturnOnEquityTTM = YahooValueFormatter.PreferExisting(
            overview.ReturnOnEquityTTM,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "Return on Equity"));

        overview.PriceToSalesRatioTTM = YahooValueFormatter.PreferExisting(
            overview.PriceToSalesRatioTTM,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "Price/Sales"));

        overview.QuarterlyRevenueGrowthYOY = YahooValueFormatter.PreferExisting(
            overview.QuarterlyRevenueGrowthYOY,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "Quarterly Revenue Growth"));

        overview.PriceAvg50 = YahooValueFormatter.PreferExisting(
            overview.PriceAvg50,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "50-Day Moving Average"));

        overview.PriceAvg200 = YahooValueFormatter.PreferExisting(
            overview.PriceAvg200,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "200-Day Moving Average"));

        overview.SharesOutstanding = YahooValueFormatter.PreferExisting(
            overview.SharesOutstanding,
            YahooHtmlValueReader.GetTableRowLastCellValue(document, "Shares Outstanding"));
    }

    private readonly IYahooJsonNavigator _jsonNavigator;
}

