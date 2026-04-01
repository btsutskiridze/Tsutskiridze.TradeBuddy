using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo;

public class YahooStockScraper : YahooScraperBase, IYahooMarketDataProvider
{
    public YahooStockScraper(
        HttpClient httpClient,
        ILogger<YahooScraperBase> logger,
        IYahooCookieBypassService yahooCookieBypassService)
        : base(httpClient, logger, yahooCookieBypassService)
    {
    }

    public async Task<bool> StockSymbolExists(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return false;

        var document = await LoadDocumentAsync($"quote/{Uri.EscapeDataString(normalizedSymbol)}", normalizedSymbol, "quote");
        if (document?.DocumentNode is null)
            return false;

        var payloadRoots = GetPayloadRoots(document);
        return FindFirstQuoteObject(payloadRoots, normalizedSymbol) is not null || IsQuoteTitleMatch(document, normalizedSymbol, out _);
    }

    public async Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return new List<StockDayPriceDto>();

        var document = await LoadDocumentAsync($"quote/{Uri.EscapeDataString(normalizedSymbol)}/history", normalizedSymbol, "history");
        if (document?.DocumentNode is null)
            return new List<StockDayPriceDto>();

        var payloadRoots = GetPayloadRoots(document);
        var prices = ParseHistoryFromJson(payloadRoots, normalizedSymbol);

        if (prices.Count == 0)
            prices = ParseHistoryFromHtml(document);

        if (days is > 0)
            prices = prices.Take(days.Value).ToList();

        return prices;
    }

    public async Task<StockOverviewDto?> GetStockOverview(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var document = await LoadDocumentAsync($"quote/{Uri.EscapeDataString(normalizedSymbol)}/key-statistics", normalizedSymbol, "key statistics");
        if (document?.DocumentNode is null)
            return null;

        var overview = new StockOverviewDto();
        var payloadRoots = GetPayloadRoots(document);

        ApplyOverviewFromJson(payloadRoots, overview);
        ApplyOverviewFromHtmlFallback(document, overview);

        return overview;
    }

    public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var document = await LoadDocumentAsync($"quote/{Uri.EscapeDataString(normalizedSymbol)}/financials", normalizedSymbol, "financials");
        if (document?.DocumentNode is null)
            return null;

        try
        {
            var report = new AnnualReportDto();
            var payloadRoots = GetPayloadRoots(document);

            ApplyAnnualReportFromJson(payloadRoots, report);
            ApplyAnnualReportFromHtmlFallback(document, report);

            if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
                report.ReportedCurrency = "USD";

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing annual report data for symbol {Symbol}", normalizedSymbol);
            return null;
        }
    }

    public async Task<StockQuoteDto?> GetStockQuote(string symbol)
    {
        if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
            return null;

        var document = await LoadDocumentAsync($"quote/{Uri.EscapeDataString(normalizedSymbol)}", normalizedSymbol, "quote");
        if (document?.DocumentNode is null)
            return null;

        var quote = new StockQuoteDto
        {
            Symbol = normalizedSymbol
        };

        var payloadRoots = GetPayloadRoots(document);
        var quoteMatchedByJson = ApplyQuoteFromJson(payloadRoots, normalizedSymbol, quote);

        if (!quoteMatchedByJson)
        {
            if (!IsQuoteTitleMatch(document, normalizedSymbol, out var titleText))
                return null;

            if (string.IsNullOrWhiteSpace(quote.Name))
                quote.Name = titleText;
        }

        ApplyQuoteFromHtmlFallback(document, quote);

        return quote;
    }

    private async Task<HtmlDocument?> LoadDocumentAsync(string relativePath, string symbol, string pageName)
    {
        try
        {
            return await GetHtmlDocumentAsync(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Yahoo {PageName} page for symbol {Symbol}", pageName, symbol);
            return null;
        }
    }

    private static bool TryNormalizeSymbol(string? symbol, out string normalizedSymbol)
    {
        normalizedSymbol = string.Empty;

        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        normalizedSymbol = symbol.Trim().ToUpperInvariant();
        return true;
    }

    private static string Clean(string? value) =>
        HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

    private static List<JsonElement> GetPayloadRoots(HtmlDocument document)
    {
        var roots = new List<JsonElement>();
        var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");

        if (scriptNodes is null)
            return roots;

        foreach (var scriptNode in scriptNodes)
        {
            var rawScript = Clean(scriptNode.InnerText);
            if (string.IsNullOrWhiteSpace(rawScript))
                continue;

            try
            {
                using var outerDocument = JsonDocument.Parse(rawScript);

                if (outerDocument.RootElement.ValueKind == JsonValueKind.Object &&
                    outerDocument.RootElement.TryGetProperty("body", out var bodyNode) &&
                    bodyNode.ValueKind == JsonValueKind.String)
                {
                    var body = bodyNode.GetString();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            using var payloadDocument = JsonDocument.Parse(body);
                            roots.Add(payloadDocument.RootElement.Clone());
                            continue;
                        }
                        catch
                        {
                            // Fall back to outer JSON below.
                        }
                    }
                }

                roots.Add(outerDocument.RootElement.Clone());
            }
            catch
            {
                // Ignore unrelated or malformed JSON blocks.
            }
        }

        return roots;
    }

    private static IEnumerable<JsonElement> Traverse(JsonElement root)
    {
        var stack = new Stack<JsonElement>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                        stack.Push(property.Value);
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                        stack.Push(item);
                }
            }
        }
    }

    private static JsonElement? FindObjectByPropertyName(JsonElement root, string propertyName)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var property in current.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Object)
                    return property.Value.Clone();
            }
        }

        return null;
    }

    private static JsonElement? FindArrayByPropertyName(JsonElement root, string propertyName)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var property in current.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Array)
                    return property.Value.Clone();
            }
        }

        return null;
    }

    private static JsonElement? FindQuoteObjectBySymbol(JsonElement root, string symbol)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            if (!current.TryGetProperty("symbol", out var symbolNode) ||
                symbolNode.ValueKind != JsonValueKind.String ||
                !string.Equals(symbolNode.GetString(), symbol, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (current.TryGetProperty("regularMarketPrice", out _) ||
                current.TryGetProperty("shortName", out _) ||
                current.TryGetProperty("longName", out _) ||
                current.TryGetProperty("quoteType", out _))
            {
                return current.Clone();
            }
        }

        return null;
    }

    private static JsonElement? FindFirstObject(IEnumerable<JsonElement> roots, string propertyName)
    {
        foreach (var root in roots)
        {
            var match = FindObjectByPropertyName(root, propertyName);
            if (match is not null)
                return match;
        }

        return null;
    }

    private static JsonElement? FindFirstQuoteObject(IEnumerable<JsonElement> roots, string symbol)
    {
        foreach (var root in roots)
        {
            var match = FindQuoteObjectBySymbol(root, symbol);
            if (match is not null)
                return match;
        }

        return null;
    }

    private static string? FindFirstEarningsDate(IEnumerable<JsonElement> roots)
    {
        foreach (var root in roots)
        {
            var calendarEvents = FindObjectByPropertyName(root, "calendarEvents");
            if (calendarEvents is null)
                continue;

            if (!calendarEvents.Value.TryGetProperty("earnings", out var earningsNode) ||
                earningsNode.ValueKind != JsonValueKind.Object ||
                !earningsNode.TryGetProperty("earningsDate", out var earningsDateNode) ||
                earningsDateNode.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in earningsDateNode.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("fmt", out var fmtNode) &&
                    fmtNode.ValueKind == JsonValueKind.String)
                {
                    return fmtNode.GetString();
                }
            }
        }

        return null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var node))
            return null;

        return node.ValueKind switch
        {
            JsonValueKind.String => node.GetString(),
            JsonValueKind.Number => node.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static string? TryGetFmtOrRawString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var node))
            return null;

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("longFmt", out var longFmtNode) && longFmtNode.ValueKind == JsonValueKind.String)
                return longFmtNode.GetString();

            if (node.TryGetProperty("fmt", out var fmtNode) && fmtNode.ValueKind == JsonValueKind.String)
                return fmtNode.GetString();

            if (node.TryGetProperty("raw", out var rawNode))
            {
                if (rawNode.ValueKind == JsonValueKind.String)
                    return rawNode.GetString();

                if (rawNode.ValueKind == JsonValueKind.Number)
                    return rawNode.GetRawText();
            }
        }

        if (node.ValueKind == JsonValueKind.String)
            return node.GetString();

        if (node.ValueKind == JsonValueKind.Number)
            return node.GetRawText();

        return null;
    }

    private static double? TryGetDoubleAt(JsonElement arrayNode, int index)
    {
        if (arrayNode.ValueKind != JsonValueKind.Array || index < 0 || index >= arrayNode.GetArrayLength())
            return null;

        var item = arrayNode[index];

        if (item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out var value))
            return value;

        if (item.ValueKind == JsonValueKind.String &&
            double.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static long? TryGetLongAt(JsonElement arrayNode, int index)
    {
        if (arrayNode.ValueKind != JsonValueKind.Array || index < 0 || index >= arrayNode.GetArrayLength())
            return null;

        var item = arrayNode[index];

        if (item.ValueKind == JsonValueKind.Number && item.TryGetInt64(out var value))
            return value;

        if (item.ValueKind == JsonValueKind.String &&
            long.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string FormatNumber(double? value) =>
        value?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;

    private static string FormatVolume(long? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    private static bool IsQuoteTitleMatch(HtmlDocument document, string symbol, out string titleText)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//section[@data-testid='quote-title']/h1");
        titleText = Clean(titleNode?.InnerText);

        return !string.IsNullOrWhiteSpace(titleText) &&
               Regex.IsMatch(
                   titleText,
                   $@"\(\s*{Regex.Escape(symbol)}\s*\)\s*$",
                   RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static void ApplyOverviewFromJson(IReadOnlyList<JsonElement> payloadRoots, StockOverviewDto overview)
    {
        var defaultKeyStatistics = FindFirstObject(payloadRoots, "defaultKeyStatistics");
        var financialData = FindFirstObject(payloadRoots, "financialData");

        if (financialData is not null)
        {
            var data = financialData.Value;
            overview.ReturnOnEquityTTM = Clean(TryGetFmtOrRawString(data, "returnOnEquity"));
            overview.QuarterlyRevenueGrowthYOY = Clean(TryGetFmtOrRawString(data, "revenueGrowth"));
        }

        if (defaultKeyStatistics is not null)
        {
            var data = defaultKeyStatistics.Value;
            overview.PriceToSalesRatioTTM = Clean(TryGetFmtOrRawString(data, "priceToSalesTrailing12Months"));
            overview.PriceAvg50 = Clean(TryGetFmtOrRawString(data, "fiftyDayAverage"));
            overview.PriceAvg200 = Clean(TryGetFmtOrRawString(data, "twoHundredDayAverage"));
            overview.SharesOutstanding = Clean(TryGetFmtOrRawString(data, "sharesOutstanding"));
        }
    }

    private static void ApplyOverviewFromHtmlFallback(HtmlDocument document, StockOverviewDto overview)
    {
        overview.ReturnOnEquityTTM = PreferExisting(
            overview.ReturnOnEquityTTM,
            GetTableRowLastCellValue(document, "Return on Equity"));

        overview.PriceToSalesRatioTTM = PreferExisting(
            overview.PriceToSalesRatioTTM,
            GetTableRowLastCellValue(document, "Price/Sales"));

        overview.QuarterlyRevenueGrowthYOY = PreferExisting(
            overview.QuarterlyRevenueGrowthYOY,
            GetTableRowLastCellValue(document, "Quarterly Revenue Growth"));

        overview.PriceAvg50 = PreferExisting(
            overview.PriceAvg50,
            GetTableRowLastCellValue(document, "50-Day Moving Average"));

        overview.PriceAvg200 = PreferExisting(
            overview.PriceAvg200,
            GetTableRowLastCellValue(document, "200-Day Moving Average"));

        overview.SharesOutstanding = PreferExisting(
            overview.SharesOutstanding,
            GetTableRowLastCellValue(document, "Shares Outstanding"));
    }

    private static void ApplyAnnualReportFromJson(IReadOnlyList<JsonElement> payloadRoots, AnnualReportDto report)
    {
        var revenue = ReadLatestAnnualMetric(payloadRoots, "annualTotalRevenue");
        var costOfRevenue = ReadLatestAnnualMetric(payloadRoots, "annualReconciledCostOfRevenue", "annualCostOfRevenue");
        var grossProfit = ReadLatestAnnualMetric(payloadRoots, "annualGrossProfit");
        var operatingIncome = ReadLatestAnnualMetric(payloadRoots, "annualOperatingIncome");
        var netIncome = ReadLatestAnnualMetric(
            payloadRoots,
            "annualNetIncomeCommonStockholders",
            "annualNetIncome",
            "annualNetIncomeIncludingNoncontrollingInterests");
        var depreciation = ReadLatestAnnualMetric(
            payloadRoots,
            "annualReconciledDepreciation",
            "annualDepreciationAndAmortization");

        if (revenue is not null)
        {
            report.TotalRevenue = revenue.Value.Value;
            report.FiscalDateEnding = revenue.Value.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            report.ReportedCurrency = revenue.Value.Currency;
        }

        if (costOfRevenue is not null)
            report.CostOfRevenue = costOfRevenue.Value.Value;

        if (grossProfit is not null)
            report.GrossProfit = grossProfit.Value.Value;

        if (operatingIncome is not null)
            report.OperatingIncome = operatingIncome.Value.Value;

        if (netIncome is not null)
            report.NetIncome = netIncome.Value.Value;

        if (depreciation is not null)
            report.DepreciationAndAmortization = depreciation.Value.Value;
    }

    private static void ApplyAnnualReportFromHtmlFallback(HtmlDocument document, AnnualReportDto report)
    {
        if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
        {
            var currencyNode = document.DocumentNode.SelectSingleNode("//span[contains(normalize-space(.), 'Currency in ')]");
            var currencyText = Clean(currencyNode?.InnerText);
            const string prefix = "Currency in ";

            if (currencyText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                report.ReportedCurrency = currencyText[prefix.Length..].Trim();
        }

        report.TotalRevenue = PreferExisting(report.TotalRevenue, GetFinancialStatementRowValue(document, "Total Revenue"));
        report.CostOfRevenue = PreferExisting(report.CostOfRevenue, GetFinancialStatementRowValue(document, "Cost of Revenue", "Reconciled Cost of Revenue"));
        report.GrossProfit = PreferExisting(report.GrossProfit, GetFinancialStatementRowValue(document, "Gross Profit"));
        report.OperatingIncome = PreferExisting(report.OperatingIncome, GetFinancialStatementRowValue(document, "Operating Income"));
        report.NetIncome = PreferExisting(
            report.NetIncome,
            GetFinancialStatementRowValue(
                document,
                "Net Income",
                "Net Income Common Stockholders",
                "Net Income Including Noncontrolling Interests",
                "Net Income from Continuing Operation Net Minority Interest"));
        report.DepreciationAndAmortization = PreferExisting(
            report.DepreciationAndAmortization,
            GetFinancialStatementRowValue(document, "Reconciled Depreciation", "Depreciation And Amortization"));
    }

    private static bool ApplyQuoteFromJson(IReadOnlyList<JsonElement> payloadRoots, string symbol, StockQuoteDto quote)
    {
        JsonElement? quoteJson = null;
        JsonElement? summaryDetailJson = null;
        JsonElement? financialDataJson = null;
        JsonElement? defaultKeyStatisticsJson = null;

        foreach (var root in payloadRoots)
        {
            quoteJson ??= FindQuoteObjectBySymbol(root, symbol);
            summaryDetailJson ??= FindObjectByPropertyName(root, "summaryDetail");
            financialDataJson ??= FindObjectByPropertyName(root, "financialData");
            defaultKeyStatisticsJson ??= FindObjectByPropertyName(root, "defaultKeyStatistics");

            if (quoteJson is not null && summaryDetailJson is not null && financialDataJson is not null && defaultKeyStatisticsJson is not null)
                break;
        }

        var earningsDate = FindFirstEarningsDate(payloadRoots);

        if (quoteJson is not null)
        {
            var data = quoteJson.Value;
            var longName = Clean(TryGetString(data, "longName"));
            var shortName = Clean(TryGetString(data, "shortName"));

            if (!string.IsNullOrWhiteSpace(longName))
                quote.Name = $"{longName} ({symbol})";
            else if (!string.IsNullOrWhiteSpace(shortName))
                quote.Name = $"{shortName} ({symbol})";

            quote.Price = Clean(TryGetFmtOrRawString(data, "regularMarketPrice"));
            quote.Change = Clean(TryGetFmtOrRawString(data, "regularMarketChange"));
            quote.ChangesPercentage = Clean(TryGetFmtOrRawString(data, "regularMarketChangePercent"));
            quote.Exchange = PreferExisting(
                quote.Exchange,
                Clean(TryGetString(data, "fullExchangeName")),
                Clean(TryGetString(data, "exchange")));
            quote.Currency = PreferExisting(quote.Currency, Clean(TryGetString(data, "currency")));
            quote.Timestamp = PreferExisting(quote.Timestamp, Clean(TryGetFmtOrRawString(data, "regularMarketTime")));
            quote.PreviousClose = PreferExisting(quote.PreviousClose, Clean(TryGetFmtOrRawString(data, "regularMarketPreviousClose")));
            quote.Open = PreferExisting(quote.Open, Clean(TryGetFmtOrRawString(data, "regularMarketOpen")));
            quote.MarketCap = PreferExisting(quote.MarketCap, Clean(TryGetFmtOrRawString(data, "marketCap")));
            quote.Volume = PreferExisting(quote.Volume, RemoveCommas(Clean(TryGetFmtOrRawString(data, "regularMarketVolume"))));
            quote.DayLow = PreferExisting(quote.DayLow, Clean(TryGetFmtOrRawString(data, "regularMarketDayLow")));
            quote.DayHigh = PreferExisting(quote.DayHigh, Clean(TryGetFmtOrRawString(data, "regularMarketDayHigh")));
            quote.YearLow = PreferExisting(quote.YearLow, Clean(TryGetFmtOrRawString(data, "fiftyTwoWeekLow")));
            quote.YearHigh = PreferExisting(quote.YearHigh, Clean(TryGetFmtOrRawString(data, "fiftyTwoWeekHigh")));
        }

        if (summaryDetailJson is not null)
        {
            var data = summaryDetailJson.Value;
            quote.PreviousClose = PreferExisting(quote.PreviousClose, Clean(TryGetFmtOrRawString(data, "regularMarketPreviousClose")));
            quote.Open = PreferExisting(quote.Open, Clean(TryGetFmtOrRawString(data, "regularMarketOpen")));
            quote.MarketCap = PreferExisting(quote.MarketCap, Clean(TryGetFmtOrRawString(data, "marketCap")));
            quote.Volume = PreferExisting(quote.Volume, RemoveCommas(Clean(TryGetFmtOrRawString(data, "regularMarketVolume"))));
            quote.AvgVolume = PreferExisting(quote.AvgVolume, RemoveCommas(Clean(TryGetFmtOrRawString(data, "averageVolume"))));
            quote.DayLow = PreferExisting(quote.DayLow, Clean(TryGetFmtOrRawString(data, "regularMarketDayLow")));
            quote.DayHigh = PreferExisting(quote.DayHigh, Clean(TryGetFmtOrRawString(data, "regularMarketDayHigh")));
            quote.YearLow = PreferExisting(quote.YearLow, Clean(TryGetFmtOrRawString(data, "fiftyTwoWeekLow")));
            quote.YearHigh = PreferExisting(quote.YearHigh, Clean(TryGetFmtOrRawString(data, "fiftyTwoWeekHigh")));
        }

        if (financialDataJson is not null)
        {
            var data = financialDataJson.Value;
            quote.Price = PreferExisting(quote.Price, Clean(TryGetFmtOrRawString(data, "currentPrice")));
            quote.Eps = PreferExisting(quote.Eps, Clean(TryGetFmtOrRawString(data, "trailingEps")));
            quote.Currency = PreferExisting(quote.Currency, Clean(TryGetString(data, "financialCurrency")));
        }

        if (defaultKeyStatisticsJson is not null)
        {
            var data = defaultKeyStatisticsJson.Value;
            quote.Eps = PreferExisting(quote.Eps, Clean(TryGetFmtOrRawString(data, "trailingEps")));

            if (string.IsNullOrWhiteSpace(quote.Pe))
            {
                var peValue = Clean(TryGetFmtOrRawString(data, "trailingPE"));
                quote.Pe = string.IsNullOrWhiteSpace(peValue) || peValue == "--" ? null : peValue;
            }
        }

        quote.EarningsAnnouncement = PreferExisting(quote.EarningsAnnouncement, earningsDate);

        return quoteJson is not null;
    }

    private static void ApplyQuoteFromHtmlFallback(HtmlDocument document, StockQuoteDto quote)
    {
        if (string.IsNullOrWhiteSpace(quote.Name) && IsQuoteTitleMatch(document, quote.Symbol, out var titleText))
            quote.Name = titleText;

        quote.Price = PreferExisting(quote.Price, GetNodeText(document, "//span[@data-testid='qsp-price']"));
        quote.Change = PreferExisting(quote.Change, GetNodeText(document, "//span[@data-testid='qsp-price-change']"));
        quote.ChangesPercentage = PreferExisting(
            quote.ChangesPercentage,
            GetNodeText(document, "//span[@data-testid='qsp-price-change-percent']").Replace("(", string.Empty).Replace(")", string.Empty));
        quote.Timestamp = PreferExisting(quote.Timestamp, GetNodeText(document, "//div[@slot='marketTimeNotice']"));
        quote.PreviousClose = PreferExisting(quote.PreviousClose, GetQuoteSummaryValue(document, "Previous Close"));
        quote.Open = PreferExisting(quote.Open, GetQuoteSummaryValue(document, "Open"));
        quote.MarketCap = PreferExisting(quote.MarketCap, GetQuoteSummaryValue(document, "Market Cap (intraday)"));
        quote.Volume = PreferExisting(quote.Volume, RemoveCommas(GetQuoteSummaryValue(document, "Volume")));
        quote.AvgVolume = PreferExisting(quote.AvgVolume, RemoveCommas(GetQuoteSummaryValue(document, "Avg. Volume")));
        quote.Eps = PreferExisting(quote.Eps, GetQuoteSummaryValue(document, "EPS (TTM)"));

        if (string.IsNullOrWhiteSpace(quote.Pe))
        {
            var peValue = GetQuoteSummaryValue(document, "PE Ratio (TTM)");
            quote.Pe = string.IsNullOrWhiteSpace(peValue) || peValue == "--" ? null : peValue;
        }

        if (string.IsNullOrWhiteSpace(quote.DayLow) || string.IsNullOrWhiteSpace(quote.DayHigh))
        {
            var (low, high) = SplitRange(GetNodeValueOrText(document, "//fin-streamer[@data-field='regularMarketDayRange']"));
            quote.DayLow = PreferExisting(quote.DayLow, low);
            quote.DayHigh = PreferExisting(quote.DayHigh, high);
        }

        if (string.IsNullOrWhiteSpace(quote.YearLow) || string.IsNullOrWhiteSpace(quote.YearHigh))
        {
            var (low, high) = SplitRange(GetNodeValueOrText(document, "//fin-streamer[@data-field='fiftyTwoWeekRange']"));
            quote.YearLow = PreferExisting(quote.YearLow, low);
            quote.YearHigh = PreferExisting(quote.YearHigh, high);
        }

        quote.EarningsAnnouncement = PreferExisting(quote.EarningsAnnouncement, GetEarningsAnnouncementFromHtml(document));

        if (string.IsNullOrWhiteSpace(quote.Exchange) || string.IsNullOrWhiteSpace(quote.Currency))
        {
            var exchangeText = GetNodeText(document, "//span[contains(@class, 'exchange')]");
            if (!string.IsNullOrWhiteSpace(exchangeText))
            {
                var parts = exchangeText.Split('•', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                    quote.Exchange = PreferExisting(quote.Exchange, parts[0]);

                if (parts.Length > 1)
                    quote.Currency = PreferExisting(quote.Currency, parts[^1]);
            }
        }
    }

    private static List<StockDayPriceDto> ParseHistoryFromJson(IReadOnlyList<JsonElement> payloadRoots, string symbol)
    {
        foreach (var root in payloadRoots)
        {
            var prices = ParseHistoryFromRoot(root, symbol);
            if (prices.Count > 0)
            {
                return prices
                    .OrderByDescending(x => x.Date)
                    .Select(x => x.Row)
                    .ToList();
            }
        }

        return new List<StockDayPriceDto>();
    }

    private static List<(DateTime Date, StockDayPriceDto Row)> ParseHistoryFromRoot(JsonElement root, string symbol)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            if (!current.TryGetProperty("meta", out var metaNode) ||
                metaNode.ValueKind != JsonValueKind.Object ||
                !metaNode.TryGetProperty("symbol", out var symbolNode) ||
                symbolNode.ValueKind != JsonValueKind.String ||
                !string.Equals(symbolNode.GetString(), symbol, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!current.TryGetProperty("timestamp", out var timestampNode) ||
                timestampNode.ValueKind != JsonValueKind.Array ||
                !current.TryGetProperty("indicators", out var indicatorsNode) ||
                indicatorsNode.ValueKind != JsonValueKind.Object ||
                !indicatorsNode.TryGetProperty("quote", out var quoteArrayNode) ||
                quoteArrayNode.ValueKind != JsonValueKind.Array ||
                quoteArrayNode.GetArrayLength() == 0)
            {
                continue;
            }

            var quoteNode = quoteArrayNode[0];
            if (quoteNode.ValueKind != JsonValueKind.Object ||
                !quoteNode.TryGetProperty("open", out var openNode) ||
                !quoteNode.TryGetProperty("high", out var highNode) ||
                !quoteNode.TryGetProperty("low", out var lowNode) ||
                !quoteNode.TryGetProperty("close", out var closeNode) ||
                !quoteNode.TryGetProperty("volume", out var volumeNode))
            {
                continue;
            }

            var rows = new List<(DateTime Date, StockDayPriceDto Row)>();
            var count = timestampNode.GetArrayLength();

            for (var index = 0; index < count; index++)
            {
                var timestamp = TryGetLongAt(timestampNode, index);
                if (timestamp is null)
                    continue;

                var open = TryGetDoubleAt(openNode, index);
                var high = TryGetDoubleAt(highNode, index);
                var low = TryGetDoubleAt(lowNode, index);
                var close = TryGetDoubleAt(closeNode, index);
                var volume = TryGetLongAt(volumeNode, index);

                if (open is null && high is null && low is null && close is null)
                    continue;

                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime.Date;
                rows.Add((date, new StockDayPriceDto
                {
                    Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Open = FormatNumber(open),
                    High = FormatNumber(high),
                    Low = FormatNumber(low),
                    Close = FormatNumber(close),
                    Volume = FormatVolume(volume)
                }));
            }

            if (rows.Count > 0)
                return rows;
        }

        return new List<(DateTime Date, StockDayPriceDto Row)>();
    }

    private List<StockDayPriceDto> ParseHistoryFromHtml(HtmlDocument document)
    {
        var prices = new List<StockDayPriceDto>();
        var tableNode = document.DocumentNode.SelectSingleNode(
            "//table[.//th[normalize-space()='Date'] and .//th[normalize-space()='Open'] and .//th[normalize-space()='Volume']]");

        if (tableNode is null)
            return prices;

        var rows = tableNode.SelectNodes(".//tbody/tr");
        if (rows is null)
            return prices;

        foreach (var row in rows)
        {
            try
            {
                var cells = row.SelectNodes("./td");
                if (cells is null || cells.Count < 7)
                    continue;

                if (!string.IsNullOrEmpty(cells[0].GetAttributeValue("colspan", string.Empty)))
                    continue;

                var rawDate = Clean(cells[0].InnerText);
                if (!DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    continue;

                prices.Add(new StockDayPriceDto
                {
                    Date = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Open = Clean(cells[1].InnerText),
                    High = Clean(cells[2].InnerText),
                    Low = Clean(cells[3].InnerText),
                    Close = Clean(cells[4].InnerText),
                    Volume = RemoveCommas(Clean(cells[6].InnerText))
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing a stock price row.");
            }
        }

        return prices;
    }

    private static AnnualMetric? ReadLatestAnnualMetric(IReadOnlyList<JsonElement> payloadRoots, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            AnnualMetric? latestMetric = null;

            foreach (var root in payloadRoots)
            {
                var metricArray = FindArrayByPropertyName(root, propertyName);
                if (metricArray is null)
                    continue;

                foreach (var item in metricArray.Value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object ||
                        !item.TryGetProperty("asOfDate", out var asOfDateNode) ||
                        !DateTime.TryParse(asOfDateNode.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                        !item.TryGetProperty("reportedValue", out var reportedValueNode) ||
                        reportedValueNode.ValueKind != JsonValueKind.Object ||
                        !reportedValueNode.TryGetProperty("fmt", out var valueNode))
                    {
                        continue;
                    }

                    var value = valueNode.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    var currency = item.TryGetProperty("currencyCode", out var currencyNode) && currencyNode.ValueKind == JsonValueKind.String
                        ? currencyNode.GetString() ?? string.Empty
                        : string.Empty;

                    if (latestMetric is null || date > latestMetric.Value.Date)
                        latestMetric = new AnnualMetric(date, currency, value);
                }
            }

            if (latestMetric is not null)
                return latestMetric;
        }

        return null;
    }

    private static string GetNodeText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return Clean(node?.InnerText);
    }

    private static string GetNodeValueOrText(HtmlDocument document, string xpath)
    {
        var node = document.DocumentNode.SelectSingleNode(xpath);
        return Clean(node?.GetAttributeValue("data-value", null) ?? node?.InnerText);
    }

    private static string GetQuoteSummaryValue(HtmlDocument document, string label)
    {
        var node = document.DocumentNode.SelectSingleNode($"//li[.//span[@title='{label}']]//fin-streamer");
        return Clean(node?.GetAttributeValue("data-value", null) ?? node?.InnerText);
    }

    private static string GetTableRowLastCellValue(HtmlDocument document, string rowLabel)
    {
        var node = document.DocumentNode.SelectSingleNode($"//tr[.//td[contains(normalize-space(.), '{rowLabel}')]]/td[last()]");
        return Clean(node?.InnerText);
    }

    private static string GetFinancialStatementRowValue(HtmlDocument document, params string[] labels)
    {
        var rowNodes = document.DocumentNode.SelectNodes("//div[contains(@class,'row')][.//div[contains(@class,'rowTitle')]]");
        if (rowNodes is null)
            return string.Empty;

        foreach (var row in rowNodes)
        {
            var rowTitleNode = row.SelectSingleNode(".//div[contains(@class,'rowTitle')]");
            if (rowTitleNode is null)
                continue;

            var label = rowTitleNode.GetAttributeValue("title", null);
            if (string.IsNullOrWhiteSpace(label))
                label = Clean(rowTitleNode.InnerText);

            if (!labels.Any(candidate => string.Equals(candidate, label, StringComparison.OrdinalIgnoreCase)))
                continue;

            var valueNodes = row.SelectNodes("./div[contains(@class,'column') and not(contains(@class,'sticky'))]");
            if (valueNodes is null)
                continue;

            foreach (var valueNode in valueNodes)
            {
                var value = Clean(valueNode.InnerText);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return string.Empty;
    }

    private static string GetEarningsAnnouncementFromHtml(HtmlDocument document)
    {
        var earningsNode = document.DocumentNode.SelectSingleNode("//li[.//span[contains(@title, 'Earnings Date')]]");
        if (earningsNode is null)
            return string.Empty;

        var finStreamer = earningsNode.SelectSingleNode(".//fin-streamer");
        if (finStreamer is not null)
            return Clean(finStreamer.InnerText);

        var spans = earningsNode.SelectNodes(".//span");
        if (spans is null)
            return string.Empty;

        var values = spans
            .Select(node => Clean(node.InnerText))
            .Where(value => !string.IsNullOrWhiteSpace(value) && !value.StartsWith("Earnings Date", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return values.Count > 0 ? values[^1] : string.Empty;
    }

    private static (string Low, string High) SplitRange(string rangeText)
    {
        if (string.IsNullOrWhiteSpace(rangeText))
            return (string.Empty, string.Empty);

        var parts = rangeText.Split(" - ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : (string.Empty, string.Empty);
    }

    private static string RemoveCommas(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace(",", string.Empty);

    private static string PreferExisting(string? currentValue, params string?[] fallbacks)
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
            return currentValue;

        foreach (var fallback in fallbacks)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;
        }

        return string.Empty;
    }

    private readonly record struct AnnualMetric(DateTime? Date, string Currency, string Value);
}
