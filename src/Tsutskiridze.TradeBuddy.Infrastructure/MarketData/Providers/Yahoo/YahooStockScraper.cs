using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Providers;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo
{
    public class YahooStockScraper : YahooScraperBase, IYahooMarketDataProvider
    {
        public YahooStockScraper(
            HttpClient httpClient,
            ILogger<YahooScraperBase> logger,
            IYahooCookieBypassService yahooCookieBypassService
        ) : base(httpClient, logger, yahooCookieBypassService)
        {
        }

        public async Task<bool> StockSymbolExists(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return false;

            symbol = symbol.Trim().ToUpperInvariant();

            HtmlDocument document;
            try
            {
                document = await GetHtmlDocumentAsync($"quote/{Uri.EscapeDataString(symbol)}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Yahoo quote page for symbol {Symbol}", symbol);
                return false;
            }

            if (document?.DocumentNode is null)
                return false;

            static string Clean(string? value) =>
                HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

            static JsonElement? FindQuoteObjectBySymbol(JsonElement root, string symbolToFind)
            {
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        if (current.TryGetProperty("symbol", out var symbolNode) &&
                            symbolNode.ValueKind == JsonValueKind.String &&
                            string.Equals(symbolNode.GetString(), symbolToFind, StringComparison.OrdinalIgnoreCase))
                        {
                            if (current.TryGetProperty("regularMarketPrice", out _) ||
                                current.TryGetProperty("shortName", out _) ||
                                current.TryGetProperty("longName", out _) ||
                                current.TryGetProperty("quoteType", out _))
                            {
                                return current;
                            }
                        }

                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return null;
            }

            var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    var rawScript = Clean(scriptNode.InnerText);
                    if (string.IsNullOrWhiteSpace(rawScript))
                        continue;

                    try
                    {
                        using var outerDoc = JsonDocument.Parse(rawScript);

                        string? payload = null;
                        if (outerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                            outerDoc.RootElement.TryGetProperty("body", out var bodyNode) &&
                            bodyNode.ValueKind == JsonValueKind.String)
                        {
                            payload = bodyNode.GetString();
                        }
                        else
                        {
                            payload = rawScript;
                        }

                        if (string.IsNullOrWhiteSpace(payload))
                            continue;

                        using var payloadDoc = JsonDocument.Parse(payload);
                        var foundQuote = FindQuoteObjectBySymbol(payloadDoc.RootElement, symbol);
                        if (foundQuote is not null)
                            return true;
                    }
                    catch
                    {
                        // Ignore non-matching JSON blocks
                    }
                }
            }

            var nameNode = document.DocumentNode.SelectSingleNode("//section[@data-testid='quote-title']/h1");
            var text = Clean(nameNode?.InnerText);

            if (string.IsNullOrWhiteSpace(text))
                return false;

            return Regex.IsMatch(
                text,
                $@"\(\s*{Regex.Escape(symbol)}\s*\)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        public async Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return new List<StockDayPriceDto>();

            symbol = symbol.Trim().ToUpperInvariant();

            HtmlDocument document;
            try
            {
                document = await GetHtmlDocumentAsync($"quote/{Uri.EscapeDataString(symbol)}/history");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Yahoo history page for symbol {Symbol}", symbol);
                return new List<StockDayPriceDto>();
            }

            if (document?.DocumentNode is null)
                return new List<StockDayPriceDto>();

            static string Clean(string? value) =>
                HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

            static double? TryGetDoubleAt(JsonElement arrayNode, int index)
            {
                if (arrayNode.ValueKind != JsonValueKind.Array)
                    return null;

                if (index < 0 || index >= arrayNode.GetArrayLength())
                    return null;

                var item = arrayNode[index];

                if (item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out var number))
                    return number;

                if (item.ValueKind == JsonValueKind.String &&
                    double.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;

                return null;
            }

            static long? TryGetLongAt(JsonElement arrayNode, int index)
            {
                if (arrayNode.ValueKind != JsonValueKind.Array)
                    return null;

                if (index < 0 || index >= arrayNode.GetArrayLength())
                    return null;

                var item = arrayNode[index];

                if (item.ValueKind == JsonValueKind.Number && item.TryGetInt64(out var number))
                    return number;

                if (item.ValueKind == JsonValueKind.String &&
                    long.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                    return parsed;

                return null;
            }

            static string FormatNumber(double? value)
            {
                return value?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;
            }

            static string FormatVolume(long? value)
            {
                return value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            }

            static List<(DateTime Date, StockDayPriceDto Row)> TryParseHistoryFromJson(JsonElement root,
                string symbolToFind)
            {
                var result = new List<(DateTime Date, StockDayPriceDto Row)>();
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        // Try Yahoo chart/spark-like structure:
                        // {
                        //   "meta": { "symbol": "MVST", ... },
                        //   "timestamp": [...],
                        //   "indicators": {
                        //      "quote": [{
                        //          "open": [...],
                        //          "high": [...],
                        //          "low": [...],
                        //          "close": [...],
                        //          "volume": [...]
                        //      }]
                        //   }
                        // }
                        if (current.TryGetProperty("meta", out var metaNode) &&
                            metaNode.ValueKind == JsonValueKind.Object &&
                            metaNode.TryGetProperty("symbol", out var symbolNode) &&
                            symbolNode.ValueKind == JsonValueKind.String &&
                            string.Equals(symbolNode.GetString(), symbolToFind, StringComparison.OrdinalIgnoreCase) &&
                            current.TryGetProperty("timestamp", out var timestampNode) &&
                            timestampNode.ValueKind == JsonValueKind.Array &&
                            current.TryGetProperty("indicators", out var indicatorsNode) &&
                            indicatorsNode.ValueKind == JsonValueKind.Object &&
                            indicatorsNode.TryGetProperty("quote", out var quoteArrayNode) &&
                            quoteArrayNode.ValueKind == JsonValueKind.Array &&
                            quoteArrayNode.GetArrayLength() > 0)
                        {
                            var quoteNode = quoteArrayNode[0];
                            if (quoteNode.ValueKind == JsonValueKind.Object &&
                                quoteNode.TryGetProperty("open", out var openNode) &&
                                quoteNode.TryGetProperty("high", out var highNode) &&
                                quoteNode.TryGetProperty("low", out var lowNode) &&
                                quoteNode.TryGetProperty("close", out var closeNode) &&
                                quoteNode.TryGetProperty("volume", out var volumeNode))
                            {
                                var count = timestampNode.GetArrayLength();

                                for (var i = 0; i < count; i++)
                                {
                                    var ts = TryGetLongAt(timestampNode, i);
                                    if (ts is null)
                                        continue;

                                    var open = TryGetDoubleAt(openNode, i);
                                    var high = TryGetDoubleAt(highNode, i);
                                    var low = TryGetDoubleAt(lowNode, i);
                                    var close = TryGetDoubleAt(closeNode, i);
                                    var volume = TryGetLongAt(volumeNode, i);

                                    // Skip rows with no price data
                                    if (open is null && high is null && low is null && close is null)
                                        continue;

                                    var date = DateTimeOffset.FromUnixTimeSeconds(ts.Value).UtcDateTime.Date;

                                    result.Add((date, new StockDayPriceDto
                                    {
                                        Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                        Open = FormatNumber(open),
                                        High = FormatNumber(high),
                                        Low = FormatNumber(low),
                                        Close = FormatNumber(close),
                                        Volume = FormatVolume(volume)
                                    }));
                                }

                                if (result.Count > 0)
                                    return result;
                            }
                        }

                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return result;
            }

            var parsedFromJson = new List<(DateTime Date, StockDayPriceDto Row)>();

            var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    var rawScript = Clean(scriptNode.InnerText);
                    if (string.IsNullOrWhiteSpace(rawScript))
                        continue;

                    try
                    {
                        using var outerDoc = JsonDocument.Parse(rawScript);

                        string? payload = null;
                        if (outerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                            outerDoc.RootElement.TryGetProperty("body", out var bodyNode) &&
                            bodyNode.ValueKind == JsonValueKind.String)
                        {
                            payload = bodyNode.GetString();
                        }
                        else
                        {
                            payload = rawScript;
                        }

                        if (string.IsNullOrWhiteSpace(payload))
                            continue;

                        // Cheap prefilter
                        if (!payload.Contains("\"timestamp\"", StringComparison.Ordinal) ||
                            !payload.Contains("\"open\"", StringComparison.Ordinal) ||
                            !payload.Contains("\"high\"", StringComparison.Ordinal) ||
                            !payload.Contains("\"low\"", StringComparison.Ordinal) ||
                            !payload.Contains("\"close\"", StringComparison.Ordinal) ||
                            !payload.Contains("\"volume\"", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        using var payloadDoc = JsonDocument.Parse(payload);
                        parsedFromJson = TryParseHistoryFromJson(payloadDoc.RootElement, symbol);

                        if (parsedFromJson.Count > 0)
                            break;
                    }
                    catch
                    {
                        // Ignore unrelated JSON blocks
                    }
                }
            }

            List<StockDayPriceDto> stockDayPrices;

            if (parsedFromJson.Count > 0)
            {
                // Keep newest first, same idea as Yahoo table order
                stockDayPrices = parsedFromJson
                    .OrderByDescending(x => x.Date)
                    .Select(x => x.Row)
                    .ToList();
            }
            else
            {
                stockDayPrices = new List<StockDayPriceDto>();

                var tableNode = document.DocumentNode.SelectSingleNode(
                    "//table[.//th[normalize-space()='Date'] and .//th[normalize-space()='Open'] and .//th[normalize-space()='Volume']]");

                if (tableNode != null)
                {
                    var rows = tableNode.SelectNodes(".//tbody/tr");
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            try
                            {
                                var cells = row.SelectNodes("./td");
                                if (cells == null || cells.Count < 7)
                                    continue;

                                // Skip summary rows like dividends/splits
                                if (!string.IsNullOrEmpty(cells[0].GetAttributeValue("colspan", "")))
                                    continue;

                                var rawDate = Clean(cells[0].InnerText);
                                if (!DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None,
                                        out var parsedDate))
                                    continue;

                                stockDayPrices.Add(new StockDayPriceDto
                                {
                                    Date = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                                    Open = Clean(cells[1].InnerText),
                                    High = Clean(cells[2].InnerText),
                                    Low = Clean(cells[3].InnerText),
                                    Close = Clean(cells[4].InnerText),
                                    Volume = Clean(cells[6].InnerText).Replace(",", "")
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error parsing a stock price row for symbol {Symbol}.", symbol);
                            }
                        }
                    }
                }
            }

            if (days.HasValue && days.Value > 0)
                stockDayPrices = stockDayPrices.Take(days.Value).ToList();

            return stockDayPrices;
        }


        public async Task<StockOverviewDto?> GetStockOverview(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            symbol = symbol.Trim().ToUpperInvariant();

            HtmlDocument document;
            try
            {
                document = await GetHtmlDocumentAsync($"quote/{Uri.EscapeDataString(symbol)}/key-statistics");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Yahoo key statistics page for symbol {Symbol}", symbol);
                return null;
            }

            if (document?.DocumentNode is null)
                return null;

            var overview = new StockOverviewDto();

            static string Clean(string? value) =>
                HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

            static JsonElement? FindObjectByPropertyName(JsonElement root, string propertyName)
            {
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.NameEquals(propertyName) && prop.Value.ValueKind == JsonValueKind.Object)
                                return prop.Value.Clone();

                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return null;
            }

            static string? TryGetFmtOrRawString(JsonElement element, string propertyName)
            {
                if (!element.TryGetProperty(propertyName, out var node))
                    return null;

                if (node.ValueKind == JsonValueKind.Object)
                {
                    if (node.TryGetProperty("longFmt", out var longFmtNode) &&
                        longFmtNode.ValueKind == JsonValueKind.String)
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

            JsonElement? defaultKeyStatisticsJson = null;
            JsonElement? financialDataJson = null;

            var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    var rawScript = Clean(scriptNode.InnerText);
                    if (string.IsNullOrWhiteSpace(rawScript))
                        continue;

                    try
                    {
                        using var outerDoc = JsonDocument.Parse(rawScript);

                        string? payload = null;
                        if (outerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                            outerDoc.RootElement.TryGetProperty("body", out var bodyNode) &&
                            bodyNode.ValueKind == JsonValueKind.String)
                        {
                            payload = bodyNode.GetString();
                        }
                        else
                        {
                            payload = rawScript;
                        }

                        if (string.IsNullOrWhiteSpace(payload))
                            continue;

                        if (!payload.Contains("defaultKeyStatistics", StringComparison.Ordinal) &&
                            !payload.Contains("financialData", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        using var payloadDoc = JsonDocument.Parse(payload);
                        var root = payloadDoc.RootElement;

                        if (defaultKeyStatisticsJson is null)
                        {
                            var foundDefaultKeyStatistics = FindObjectByPropertyName(root, "defaultKeyStatistics");
                            if (foundDefaultKeyStatistics is not null)
                                defaultKeyStatisticsJson = foundDefaultKeyStatistics.Value;
                        }

                        if (financialDataJson is null)
                        {
                            var foundFinancialData = FindObjectByPropertyName(root, "financialData");
                            if (foundFinancialData is not null)
                                financialDataJson = foundFinancialData.Value;
                        }

                        if (defaultKeyStatisticsJson is not null && financialDataJson is not null)
                            break;
                    }
                    catch
                    {
                        // Ignore unrelated JSON blocks
                    }
                }
            }

            // JSON first

            if (financialDataJson is not null)
            {
                var financialData = financialDataJson.Value;

                overview.ReturnOnEquityTTM = Clean(TryGetFmtOrRawString(financialData, "returnOnEquity"));
                overview.QuarterlyRevenueGrowthYOY = Clean(TryGetFmtOrRawString(financialData, "revenueGrowth"));
            }

            if (defaultKeyStatisticsJson is not null)
            {
                var defaultKeyStatistics = defaultKeyStatisticsJson.Value;

                overview.PriceToSalesRatioTTM =
                    Clean(TryGetFmtOrRawString(defaultKeyStatistics, "priceToSalesTrailing12Months"));
                overview.PriceAvg50 = Clean(TryGetFmtOrRawString(defaultKeyStatistics, "fiftyDayAverage"));
                overview.PriceAvg200 = Clean(TryGetFmtOrRawString(defaultKeyStatistics, "twoHundredDayAverage"));
                overview.SharesOutstanding = Clean(TryGetFmtOrRawString(defaultKeyStatistics, "sharesOutstanding"));
            }

            // HTML fallback only for missing values

            if (string.IsNullOrWhiteSpace(overview.ReturnOnEquityTTM))
            {
                var roeNode = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), 'Return on Equity')]]/td[last()]");
                overview.ReturnOnEquityTTM = Clean(roeNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(overview.PriceToSalesRatioTTM))
            {
                var psNode = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), 'Price/Sales')]]/td[last()]");
                overview.PriceToSalesRatioTTM = Clean(psNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(overview.QuarterlyRevenueGrowthYOY))
            {
                var qrgNode = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), 'Quarterly Revenue Growth')]]/td[last()]");
                overview.QuarterlyRevenueGrowthYOY = Clean(qrgNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(overview.PriceAvg50))
            {
                var ma50Node = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), '50-Day Moving Average')]]/td[last()]");
                overview.PriceAvg50 = Clean(ma50Node?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(overview.PriceAvg200))
            {
                var ma200Node = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), '200-Day Moving Average')]]/td[last()]");
                overview.PriceAvg200 = Clean(ma200Node?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(overview.SharesOutstanding))
            {
                var soNode = document.DocumentNode.SelectSingleNode(
                    "//tr[.//td[contains(normalize-space(.), 'Shares Outstanding')]]/td[last()]");
                overview.SharesOutstanding = Clean(soNode?.InnerText);
            }

            return overview;
        }

        public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            symbol = symbol.Trim().ToUpperInvariant();

            HtmlDocument document;
            try
            {
                document = await GetHtmlDocumentAsync($"quote/{Uri.EscapeDataString(symbol)}/financials");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Yahoo financials page for symbol {Symbol}", symbol);
                return null;
            }

            if (document?.DocumentNode is null)
                return null;

            var report = new AnnualReportDto();

            try
            {
                var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");

                JsonElement? FindArrayByName(JsonElement root, string propertyName)
                {
                    var stack = new Stack<JsonElement>();
                    stack.Push(root);

                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();

                        if (current.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in current.EnumerateObject())
                            {
                                if (prop.NameEquals(propertyName) && prop.Value.ValueKind == JsonValueKind.Array)
                                    return prop.Value;

                                if (prop.Value.ValueKind == JsonValueKind.Object ||
                                    prop.Value.ValueKind == JsonValueKind.Array)
                                    stack.Push(prop.Value);
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

                    return null;
                }

                (DateTime? Date, string? Currency, string? Value) ReadLatestMetric(JsonElement root,
                    params string[] propertyNames)
                {
                    DateTime? latestDate = null;
                    string? latestCurrency = null;
                    string? latestValue = null;

                    foreach (var propertyName in propertyNames)
                    {
                        var arrayNode = FindArrayByName(root, propertyName);
                        if (arrayNode is null || arrayNode.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (var item in arrayNode.Value.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Object)
                                continue;

                            if (!item.TryGetProperty("asOfDate", out var asOfDateNode))
                                continue;

                            var asOfDateText = asOfDateNode.GetString();
                            if (!DateTime.TryParse(asOfDateText, CultureInfo.InvariantCulture, DateTimeStyles.None,
                                    out var asOfDate))
                                continue;

                            string? fmtValue = null;

                            if (item.TryGetProperty("reportedValue", out var reportedValueNode) &&
                                reportedValueNode.ValueKind == JsonValueKind.Object &&
                                reportedValueNode.TryGetProperty("fmt", out var fmtNode))
                            {
                                fmtValue = fmtNode.GetString();
                            }

                            if (string.IsNullOrWhiteSpace(fmtValue))
                                continue;

                            string? currency = null;
                            if (item.TryGetProperty("currencyCode", out var currencyNode) &&
                                currencyNode.ValueKind == JsonValueKind.String)
                            {
                                currency = currencyNode.GetString();
                            }

                            if (latestDate is null || asOfDate > latestDate.Value)
                            {
                                latestDate = asOfDate;
                                latestCurrency = currency;
                                latestValue = fmtValue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(latestValue))
                            break;
                    }

                    return (latestDate, latestCurrency, latestValue);
                }

                if (scriptNodes != null)
                {
                    foreach (var scriptNode in scriptNodes)
                    {
                        var rawScript = HtmlEntity.DeEntitize(scriptNode.InnerText ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(rawScript))
                            continue;

                        try
                        {
                            using var outerDoc = JsonDocument.Parse(rawScript);

                            string? jsonPayload = null;

                            if (outerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                                outerDoc.RootElement.TryGetProperty("body", out var bodyNode) &&
                                bodyNode.ValueKind == JsonValueKind.String)
                            {
                                jsonPayload = bodyNode.GetString();
                            }
                            else
                            {
                                jsonPayload = rawScript;
                            }

                            if (string.IsNullOrWhiteSpace(jsonPayload))
                                continue;

                            if (!jsonPayload.Contains("annual", StringComparison.OrdinalIgnoreCase) ||
                                !jsonPayload.Contains("Revenue", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            using var payloadDoc = JsonDocument.Parse(jsonPayload);
                            var payloadRoot = payloadDoc.RootElement;

                            var revenue = ReadLatestMetric(payloadRoot, "annualTotalRevenue");
                            var costOfRevenue = ReadLatestMetric(payloadRoot, "annualReconciledCostOfRevenue",
                                "annualCostOfRevenue");
                            var grossProfit = ReadLatestMetric(payloadRoot, "annualGrossProfit");
                            var operatingIncome = ReadLatestMetric(payloadRoot, "annualOperatingIncome");
                            var netIncome = ReadLatestMetric(payloadRoot,
                                "annualNetIncomeCommonStockholders",
                                "annualNetIncome",
                                "annualNetIncomeIncludingNoncontrollingInterests");
                            var depreciation = ReadLatestMetric(payloadRoot,
                                "annualReconciledDepreciation",
                                "annualDepreciationAndAmortization");

                            if (!string.IsNullOrWhiteSpace(revenue.Value))
                            {
                                report.TotalRevenue = revenue.Value ?? "";
                                report.FiscalDateEnding = revenue.Date?.ToString("yyyy-MM-dd") ?? "";
                                report.ReportedCurrency = revenue.Currency ?? "";
                            }

                            if (!string.IsNullOrWhiteSpace(costOfRevenue.Value))
                                report.CostOfRevenue = costOfRevenue.Value ?? "";

                            if (!string.IsNullOrWhiteSpace(grossProfit.Value))
                                report.GrossProfit = grossProfit.Value ?? "";

                            if (!string.IsNullOrWhiteSpace(operatingIncome.Value))
                                report.OperatingIncome = operatingIncome.Value ?? "";

                            if (!string.IsNullOrWhiteSpace(netIncome.Value))
                                report.NetIncome = netIncome.Value ?? "";

                            if (!string.IsNullOrWhiteSpace(depreciation.Value))
                                report.DepreciationAndAmortization = depreciation.Value ?? "";

                            var hasMainData =
                                !string.IsNullOrWhiteSpace(report.TotalRevenue) ||
                                !string.IsNullOrWhiteSpace(report.GrossProfit) ||
                                !string.IsNullOrWhiteSpace(report.OperatingIncome) ||
                                !string.IsNullOrWhiteSpace(report.NetIncome);

                            if (hasMainData)
                                break;
                        }
                        catch
                        {
                            // Ignore non-matching JSON blocks
                        }
                    }
                }

                // HTML fallback only for missing values

                if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
                {
                    var currencyNode =
                        document.DocumentNode.SelectSingleNode("//span[contains(normalize-space(.), 'Currency in ')]");
                    if (currencyNode != null)
                    {
                        var currencyText = HtmlEntity.DeEntitize(currencyNode.InnerText ?? string.Empty).Trim();
                        const string prefix = "Currency in ";
                        if (currencyText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                            report.ReportedCurrency = currencyText[prefix.Length..].Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(report.TotalRevenue) ||
                    string.IsNullOrWhiteSpace(report.CostOfRevenue) ||
                    string.IsNullOrWhiteSpace(report.GrossProfit) ||
                    string.IsNullOrWhiteSpace(report.OperatingIncome) ||
                    string.IsNullOrWhiteSpace(report.NetIncome) ||
                    string.IsNullOrWhiteSpace(report.DepreciationAndAmortization))
                {
                    var rowNodes =
                        document.DocumentNode.SelectNodes(
                            "//div[contains(@class,'row')][.//div[contains(@class,'rowTitle')]]");

                    if (rowNodes != null)
                    {
                        foreach (var row in rowNodes)
                        {
                            var rowTitleNode = row.SelectSingleNode(".//div[contains(@class,'rowTitle')]");
                            if (rowTitleNode == null)
                                continue;

                            var label =
                                rowTitleNode.GetAttributeValue("title", null) ??
                                HtmlEntity.DeEntitize(rowTitleNode.InnerText ?? string.Empty).Trim();

                            if (string.IsNullOrWhiteSpace(label))
                                continue;

                            var valueNodes =
                                row.SelectNodes("./div[contains(@class,'column') and not(contains(@class,'sticky'))]");
                            if (valueNodes == null || valueNodes.Count == 0)
                                continue;

                            string firstValue = "";
                            foreach (var valueNode in valueNodes)
                            {
                                var candidate = HtmlEntity.DeEntitize(valueNode.InnerText ?? string.Empty).Trim();
                                if (!string.IsNullOrWhiteSpace(candidate))
                                {
                                    firstValue = candidate;
                                    break;
                                }
                            }

                            if (string.IsNullOrWhiteSpace(firstValue))
                                continue;

                            switch (label)
                            {
                                case "Total Revenue":
                                    if (string.IsNullOrWhiteSpace(report.TotalRevenue))
                                        report.TotalRevenue = firstValue;
                                    break;

                                case "Cost of Revenue":
                                case "Reconciled Cost of Revenue":
                                    if (string.IsNullOrWhiteSpace(report.CostOfRevenue))
                                        report.CostOfRevenue = firstValue;
                                    break;

                                case "Gross Profit":
                                    if (string.IsNullOrWhiteSpace(report.GrossProfit))
                                        report.GrossProfit = firstValue;
                                    break;

                                case "Operating Income":
                                    if (string.IsNullOrWhiteSpace(report.OperatingIncome))
                                        report.OperatingIncome = firstValue;
                                    break;

                                case "Net Income":
                                case "Net Income Common Stockholders":
                                case "Net Income Including Noncontrolling Interests":
                                case "Net Income from Continuing Operation Net Minority Interest":
                                    if (string.IsNullOrWhiteSpace(report.NetIncome))
                                        report.NetIncome = firstValue;
                                    break;

                                case "Reconciled Depreciation":
                                case "Depreciation And Amortization":
                                    if (string.IsNullOrWhiteSpace(report.DepreciationAndAmortization))
                                        report.DepreciationAndAmortization = firstValue;
                                    break;
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(report.ReportedCurrency))
                    report.ReportedCurrency = "USD";

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing annual report data for symbol {Symbol}", symbol);
                return null;
            }
        }

        public async Task<StockQuoteDto?> GetStockQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            symbol = symbol.Trim().ToUpperInvariant();

            HtmlDocument document;
            try
            {
                document = await GetHtmlDocumentAsync($"quote/{Uri.EscapeDataString(symbol)}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load Yahoo quote page for symbol {Symbol}", symbol);
                return null;
            }

            if (document?.DocumentNode is null)
                return null;

            var quote = new StockQuoteDto
            {
                Symbol = symbol
            };

            static string Clean(string? value) =>
                HtmlEntity.DeEntitize(value ?? string.Empty).Trim();

            static string? TryGetString(JsonElement element, string propertyName)
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

            static string? TryGetFmtOrRawString(JsonElement element, string propertyName)
            {
                if (!element.TryGetProperty(propertyName, out var node))
                    return null;

                if (node.ValueKind == JsonValueKind.Object)
                {
                    if (node.TryGetProperty("longFmt", out var longFmtNode) &&
                        longFmtNode.ValueKind == JsonValueKind.String)
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

            static JsonElement? FindObjectByPropertyName(JsonElement root, string propertyName)
            {
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.NameEquals(propertyName) && prop.Value.ValueKind == JsonValueKind.Object)
                                return prop.Value;

                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return null;
            }

            static JsonElement? FindQuoteObjectBySymbol(JsonElement root, string symbolToFind)
            {
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        if (current.TryGetProperty("symbol", out var symbolNode) &&
                            symbolNode.ValueKind == JsonValueKind.String &&
                            string.Equals(symbolNode.GetString(), symbolToFind, StringComparison.OrdinalIgnoreCase))
                        {
                            if (current.TryGetProperty("regularMarketPrice", out _) ||
                                current.TryGetProperty("shortName", out _) ||
                                current.TryGetProperty("longName", out _))
                            {
                                return current;
                            }
                        }

                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return null;
            }

            static string? FindFirstMetricFmt(JsonElement root, string propertyName)
            {
                var stack = new Stack<JsonElement>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in current.EnumerateObject())
                        {
                            if (prop.NameEquals(propertyName))
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    if (prop.Value.TryGetProperty("longFmt", out var longFmtNode) &&
                                        longFmtNode.ValueKind == JsonValueKind.String)
                                        return longFmtNode.GetString();

                                    if (prop.Value.TryGetProperty("fmt", out var fmtNode) &&
                                        fmtNode.ValueKind == JsonValueKind.String)
                                        return fmtNode.GetString();

                                    if (prop.Value.TryGetProperty("raw", out var rawNode))
                                    {
                                        if (rawNode.ValueKind == JsonValueKind.String)
                                            return rawNode.GetString();

                                        if (rawNode.ValueKind == JsonValueKind.Number)
                                            return rawNode.GetRawText();
                                    }
                                }
                                else if (prop.Value.ValueKind == JsonValueKind.String)
                                {
                                    return prop.Value.GetString();
                                }
                                else if (prop.Value.ValueKind == JsonValueKind.Number)
                                {
                                    return prop.Value.GetRawText();
                                }
                            }

                            if (prop.Value.ValueKind == JsonValueKind.Object ||
                                prop.Value.ValueKind == JsonValueKind.Array)
                                stack.Push(prop.Value);
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

                return null;
            }

            static string? FindFirstEarningsDate(JsonElement root)
            {
                var calendarEvents = FindObjectByPropertyName(root, "calendarEvents");
                if (calendarEvents is null || calendarEvents.Value.ValueKind != JsonValueKind.Object)
                    return null;

                if (!calendarEvents.Value.TryGetProperty("earnings", out var earningsNode) ||
                    earningsNode.ValueKind != JsonValueKind.Object)
                    return null;

                if (!earningsNode.TryGetProperty("earningsDate", out var earningsDateNode) ||
                    earningsDateNode.ValueKind != JsonValueKind.Array)
                    return null;

                foreach (var item in earningsDateNode.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                        continue;

                    if (item.TryGetProperty("fmt", out var fmtNode) && fmtNode.ValueKind == JsonValueKind.String)
                        return fmtNode.GetString();
                }

                return null;
            }

            JsonElement? quoteJson = null;
            JsonElement? summaryDetailJson = null;
            JsonElement? financialDataJson = null;
            JsonElement? defaultKeyStatisticsJson = null;
            string? earningsDateJson = null;

            var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");
            if (scriptNodes != null)
            {
                foreach (var scriptNode in scriptNodes)
                {
                    var rawScript = Clean(scriptNode.InnerText);
                    if (string.IsNullOrWhiteSpace(rawScript))
                        continue;

                    try
                    {
                        using (var outerDoc = JsonDocument.Parse(rawScript))
                        {
                            string? payload = null;
                            if (outerDoc.RootElement.ValueKind == JsonValueKind.Object &&
                                outerDoc.RootElement.TryGetProperty("body", out var bodyNode) &&
                                bodyNode.ValueKind == JsonValueKind.String)
                            {
                                payload = bodyNode.GetString();
                            }
                            else
                            {
                                payload = rawScript;
                            }

                            if (string.IsNullOrWhiteSpace(payload))
                                continue;

                            using var payloadDoc = JsonDocument.Parse(payload);
                            var root = payloadDoc.RootElement;

                            if (quoteJson is null)
                            {
                                var foundQuote = FindQuoteObjectBySymbol(root, symbol);
                                if (foundQuote is not null)
                                    quoteJson = foundQuote.Value.Clone();
                            }

                            if (summaryDetailJson is null)
                            {
                                var foundSummaryDetail = FindObjectByPropertyName(root, "summaryDetail");
                                if (foundSummaryDetail is not null)
                                    summaryDetailJson = foundSummaryDetail.Value.Clone();
                            }

                            if (financialDataJson is null)
                            {
                                var foundFinancialData = FindObjectByPropertyName(root, "financialData");
                                if (foundFinancialData is not null)
                                    financialDataJson = foundFinancialData.Value.Clone();
                            }

                            if (defaultKeyStatisticsJson is null)
                            {
                                var foundDefaultKeyStatistics = FindObjectByPropertyName(root, "defaultKeyStatistics");
                                if (foundDefaultKeyStatistics is not null)
                                    defaultKeyStatisticsJson = foundDefaultKeyStatistics.Value.Clone();
                            }

                            if (earningsDateJson is null)
                            {
                                earningsDateJson = FindFirstEarningsDate(root);
                            }

                            if (quoteJson is not null &&
                                summaryDetailJson is not null &&
                                financialDataJson is not null &&
                                defaultKeyStatisticsJson is not null &&
                                !string.IsNullOrWhiteSpace(earningsDateJson))
                            {
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore non-matching JSON blocks
                    }
                }
            }

            // JSON first

            if (quoteJson is not null)
            {
                var q = quoteJson.Value;

                var longName = Clean(TryGetString(q, "longName"));
                var shortName = Clean(TryGetString(q, "shortName"));

                if (!string.IsNullOrWhiteSpace(longName))
                    quote.Name = $"{longName} ({symbol})";
                else if (!string.IsNullOrWhiteSpace(shortName))
                    quote.Name = $"{shortName} ({symbol})";

                quote.Price = Clean(TryGetFmtOrRawString(q, "regularMarketPrice"));
                quote.Change = Clean(TryGetFmtOrRawString(q, "regularMarketChange"));
                quote.ChangesPercentage = Clean(TryGetFmtOrRawString(q, "regularMarketChangePercent"));

                quote.Exchange = Clean(TryGetString(q, "fullExchangeName")) switch
                {
                    var full when !string.IsNullOrWhiteSpace(full) => full,
                    _ => Clean(TryGetString(q, "exchange"))
                };

                quote.Currency = Clean(TryGetString(q, "currency"));
                quote.Timestamp = Clean(TryGetFmtOrRawString(q, "regularMarketTime"));
                quote.PreviousClose = Clean(TryGetFmtOrRawString(q, "regularMarketPreviousClose"));
                quote.Open = Clean(TryGetFmtOrRawString(q, "regularMarketOpen"));
                quote.MarketCap = Clean(TryGetFmtOrRawString(q, "marketCap"));

                var volumeText = Clean(TryGetFmtOrRawString(q, "regularMarketVolume"));
                if (!string.IsNullOrWhiteSpace(volumeText))
                    quote.Volume = volumeText.Replace(",", "");

                var dayLow = Clean(TryGetFmtOrRawString(q, "regularMarketDayLow"));
                var dayHigh = Clean(TryGetFmtOrRawString(q, "regularMarketDayHigh"));
                if (!string.IsNullOrWhiteSpace(dayLow))
                    quote.DayLow = dayLow;
                if (!string.IsNullOrWhiteSpace(dayHigh))
                    quote.DayHigh = dayHigh;

                var yearLow = Clean(TryGetFmtOrRawString(q, "fiftyTwoWeekLow"));
                var yearHigh = Clean(TryGetFmtOrRawString(q, "fiftyTwoWeekHigh"));
                if (!string.IsNullOrWhiteSpace(yearLow))
                    quote.YearLow = yearLow;
                if (!string.IsNullOrWhiteSpace(yearHigh))
                    quote.YearHigh = yearHigh;
            }

            if (summaryDetailJson is not null)
            {
                var s = summaryDetailJson.Value;

                if (string.IsNullOrWhiteSpace(quote.PreviousClose))
                    quote.PreviousClose = Clean(TryGetFmtOrRawString(s, "regularMarketPreviousClose"));

                if (string.IsNullOrWhiteSpace(quote.Open))
                    quote.Open = Clean(TryGetFmtOrRawString(s, "regularMarketOpen"));

                if (string.IsNullOrWhiteSpace(quote.MarketCap))
                    quote.MarketCap = Clean(TryGetFmtOrRawString(s, "marketCap"));

                if (string.IsNullOrWhiteSpace(quote.Volume))
                {
                    var volumeText = Clean(TryGetFmtOrRawString(s, "regularMarketVolume"));
                    if (!string.IsNullOrWhiteSpace(volumeText))
                        quote.Volume = volumeText.Replace(",", "");
                }

                if (string.IsNullOrWhiteSpace(quote.AvgVolume))
                {
                    var avgVolumeText = Clean(TryGetFmtOrRawString(s, "averageVolume"));
                    if (!string.IsNullOrWhiteSpace(avgVolumeText))
                        quote.AvgVolume = avgVolumeText.Replace(",", "");
                }

                if (string.IsNullOrWhiteSpace(quote.DayLow))
                    quote.DayLow = Clean(TryGetFmtOrRawString(s, "regularMarketDayLow"));

                if (string.IsNullOrWhiteSpace(quote.DayHigh))
                    quote.DayHigh = Clean(TryGetFmtOrRawString(s, "regularMarketDayHigh"));

                if (string.IsNullOrWhiteSpace(quote.YearLow))
                    quote.YearLow = Clean(TryGetFmtOrRawString(s, "fiftyTwoWeekLow"));

                if (string.IsNullOrWhiteSpace(quote.YearHigh))
                    quote.YearHigh = Clean(TryGetFmtOrRawString(s, "fiftyTwoWeekHigh"));
            }

            if (financialDataJson is not null)
            {
                var f = financialDataJson.Value;

                if (string.IsNullOrWhiteSpace(quote.Price))
                    quote.Price = Clean(TryGetFmtOrRawString(f, "currentPrice"));

                if (string.IsNullOrWhiteSpace(quote.Eps))
                    quote.Eps = Clean(TryGetFmtOrRawString(f, "trailingEps"));

                if (string.IsNullOrWhiteSpace(quote.Currency))
                    quote.Currency = Clean(TryGetString(f, "financialCurrency"));
            }

            if (defaultKeyStatisticsJson is not null)
            {
                var d = defaultKeyStatisticsJson.Value;

                if (string.IsNullOrWhiteSpace(quote.Eps))
                    quote.Eps = Clean(TryGetFmtOrRawString(d, "trailingEps"));

                if (string.IsNullOrWhiteSpace(quote.Pe))
                {
                    var peText = Clean(TryGetFmtOrRawString(d, "trailingPE"));
                    quote.Pe = string.IsNullOrWhiteSpace(peText) || peText == "--" ? null : peText;
                }
            }

            if (string.IsNullOrWhiteSpace(quote.EarningsAnnouncement) && !string.IsNullOrWhiteSpace(earningsDateJson))
                quote.EarningsAnnouncement = earningsDateJson;

            // Validate symbol existence from JSON-first result
            if (quoteJson is null)
            {
                var nameNode = document.DocumentNode.SelectSingleNode("//section[@data-testid='quote-title']/h1");
                var titleText = Clean(nameNode?.InnerText);

                if (string.IsNullOrWhiteSpace(titleText) ||
                    !Regex.IsMatch(
                        titleText,
                        $@"\(\s*{Regex.Escape(symbol)}\s*\)\s*$",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(quote.Name))
                    quote.Name = titleText;
            }

            // HTML fallback only for missing fields

            if (string.IsNullOrWhiteSpace(quote.Price))
            {
                var priceNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price']");
                quote.Price = Clean(priceNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.Change))
            {
                var changeNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price-change']");
                quote.Change = Clean(changeNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.ChangesPercentage))
            {
                var changePercentNode =
                    document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price-change-percent']");
                quote.ChangesPercentage = Clean(changePercentNode?.InnerText)
                    .Replace("(", "")
                    .Replace(")", "");
            }

            if (string.IsNullOrWhiteSpace(quote.Timestamp))
            {
                var timestampNode = document.DocumentNode.SelectSingleNode("//div[@slot='marketTimeNotice']");
                quote.Timestamp = Clean(timestampNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.PreviousClose))
            {
                var prevCloseNode =
                    document.DocumentNode.SelectSingleNode("//li[.//span[@title='Previous Close']]//fin-streamer");
                quote.PreviousClose =
                    Clean(prevCloseNode?.GetAttributeValue("data-value", null) ?? prevCloseNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.Open))
            {
                var openNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Open']]//fin-streamer");
                quote.Open = Clean(openNode?.GetAttributeValue("data-value", null) ?? openNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.DayLow) || string.IsNullOrWhiteSpace(quote.DayHigh))
            {
                var dayRangeNode =
                    document.DocumentNode.SelectSingleNode("//fin-streamer[@data-field='regularMarketDayRange']");
                var dayRangeText =
                    Clean(dayRangeNode?.GetAttributeValue("data-value", null) ?? dayRangeNode?.InnerText);
                if (!string.IsNullOrWhiteSpace(dayRangeText))
                {
                    var parts = dayRangeText.Split(" - ",
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        if (string.IsNullOrWhiteSpace(quote.DayLow))
                            quote.DayLow = parts[0];

                        if (string.IsNullOrWhiteSpace(quote.DayHigh))
                            quote.DayHigh = parts[1];
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(quote.YearLow) || string.IsNullOrWhiteSpace(quote.YearHigh))
            {
                var yearRangeNode =
                    document.DocumentNode.SelectSingleNode("//fin-streamer[@data-field='fiftyTwoWeekRange']");
                var yearRangeText =
                    Clean(yearRangeNode?.GetAttributeValue("data-value", null) ?? yearRangeNode?.InnerText);
                if (!string.IsNullOrWhiteSpace(yearRangeText))
                {
                    var parts = yearRangeText.Split(" - ",
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        if (string.IsNullOrWhiteSpace(quote.YearLow))
                            quote.YearLow = parts[0];

                        if (string.IsNullOrWhiteSpace(quote.YearHigh))
                            quote.YearHigh = parts[1];
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(quote.MarketCap))
            {
                var marketCapNode =
                    document.DocumentNode.SelectSingleNode(
                        "//li[.//span[@title='Market Cap (intraday)']]//fin-streamer");
                quote.MarketCap = Clean(marketCapNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.Volume))
            {
                var volumeNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Volume']]//fin-streamer");
                quote.Volume = Clean(volumeNode?.InnerText).Replace(",", "");
            }

            if (string.IsNullOrWhiteSpace(quote.AvgVolume))
            {
                var avgVolumeNode =
                    document.DocumentNode.SelectSingleNode("//li[.//span[@title='Avg. Volume']]//fin-streamer");
                quote.AvgVolume = Clean(avgVolumeNode?.InnerText).Replace(",", "");
            }

            if (string.IsNullOrWhiteSpace(quote.Eps))
            {
                var epsNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='EPS (TTM)']]//fin-streamer");
                quote.Eps = Clean(epsNode?.InnerText);
            }

            if (string.IsNullOrWhiteSpace(quote.Pe))
            {
                var peNode =
                    document.DocumentNode.SelectSingleNode("//li[.//span[@title='PE Ratio (TTM)']]//fin-streamer");
                var peText = Clean(peNode?.InnerText);
                quote.Pe = string.IsNullOrWhiteSpace(peText) || peText == "--" ? null : peText;
            }

            if (string.IsNullOrWhiteSpace(quote.EarningsAnnouncement))
            {
                var earningsNode =
                    document.DocumentNode.SelectSingleNode("//li[.//span[contains(@title, 'Earnings Date')]]");
                if (earningsNode != null)
                {
                    var earningsFinStreamer = earningsNode.SelectSingleNode(".//fin-streamer");
                    if (earningsFinStreamer != null)
                    {
                        quote.EarningsAnnouncement = Clean(earningsFinStreamer.InnerText);
                    }
                    else
                    {
                        var spans = earningsNode.SelectNodes(".//span");
                        if (spans != null)
                        {
                            var values = spans
                                .Select(x => Clean(x.InnerText))
                                .Where(x => !string.IsNullOrWhiteSpace(x) &&
                                            !x.StartsWith("Earnings Date", StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (values.Count > 0)
                                quote.EarningsAnnouncement = values[^1];
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(quote.Exchange) || string.IsNullOrWhiteSpace(quote.Currency))
            {
                var exchangeNode = document.DocumentNode.SelectSingleNode("//span[contains(@class, 'exchange')]");
                if (exchangeNode != null)
                {
                    var exchangeText = Clean(exchangeNode.InnerText);
                    var parts = exchangeText.Split('•',
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                    if (string.IsNullOrWhiteSpace(quote.Exchange) && parts.Length > 0)
                        quote.Exchange = parts[0];

                    if (string.IsNullOrWhiteSpace(quote.Currency) && parts.Length > 1)
                        quote.Currency = parts[^1];
                }
            }

            return quote;
        }
    }
}