using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing;

public class YahooHistoryPageParser : IYahooHistoryPageParser
{
    public YahooHistoryPageParser(
        IYahooJsonNavigator jsonNavigator,
        ILogger<YahooHistoryPageParser> logger)
    {
        _jsonNavigator = jsonNavigator;
        _logger = logger;
    }

    public List<StockDayPriceDto> Parse(YahooPageContext pageContext)
    {
        var prices = ParseHistoryFromJson(pageContext.PayloadRoots, pageContext.Symbol);
        return prices.Count > 0 ? prices : ParseHistoryFromHtml(pageContext.Document);
    }

    private List<StockDayPriceDto> ParseHistoryFromJson(IReadOnlyList<JsonElement> payloadRoots, string symbol)
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

    private List<(DateTime Date, StockDayPriceDto Row)> ParseHistoryFromRoot(JsonElement root, string symbol)
    {
        foreach (var current in _jsonNavigator.Traverse(root))
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
                var timestamp = _jsonNavigator.TryGetLongAt(timestampNode, index);
                if (timestamp is null)
                    continue;

                var open = _jsonNavigator.TryGetDoubleAt(openNode, index);
                var high = _jsonNavigator.TryGetDoubleAt(highNode, index);
                var low = _jsonNavigator.TryGetDoubleAt(lowNode, index);
                var close = _jsonNavigator.TryGetDoubleAt(closeNode, index);
                var volume = _jsonNavigator.TryGetLongAt(volumeNode, index);

                if (open is null && high is null && low is null && close is null)
                    continue;

                var date = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value).UtcDateTime.Date;
                rows.Add((date, new StockDayPriceDto
                {
                    Date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Open = YahooValueFormatter.FormatNumber(open),
                    High = YahooValueFormatter.FormatNumber(high),
                    Low = YahooValueFormatter.FormatNumber(low),
                    Close = YahooValueFormatter.FormatNumber(close),
                    Volume = YahooValueFormatter.FormatVolume(volume)
                }));
            }

            if (rows.Count > 0)
                return rows;
        }

        return new List<(DateTime Date, StockDayPriceDto Row)>();
    }

    private List<StockDayPriceDto> ParseHistoryFromHtml(HtmlAgilityPack.HtmlDocument document)
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

                var rawDate = YahooHtmlValueReader.Clean(cells[0].InnerText);
                if (!DateTime.TryParse(rawDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    continue;

                prices.Add(new StockDayPriceDto
                {
                    Date = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Open = YahooHtmlValueReader.Clean(cells[1].InnerText),
                    High = YahooHtmlValueReader.Clean(cells[2].InnerText),
                    Low = YahooHtmlValueReader.Clean(cells[3].InnerText),
                    Close = YahooHtmlValueReader.Clean(cells[4].InnerText),
                    Volume = YahooValueFormatter.RemoveCommas(YahooHtmlValueReader.Clean(cells[6].InnerText))
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing a stock price row.");
            }
        }

        return prices;
    }

    private readonly IYahooJsonNavigator _jsonNavigator;
    private readonly ILogger<YahooHistoryPageParser> _logger;
}

