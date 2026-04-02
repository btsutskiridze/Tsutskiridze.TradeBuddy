using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Helpers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.News.Yahoo
{
    public class YahooNewsScraper : IYahooNewsProvider
    {
        private readonly IYahooPageLoader _pageLoader;
        private readonly IYahooJsonNavigator _jsonNavigator;
        private readonly ILogger<YahooNewsScraper> _logger;
    
        public YahooNewsScraper(        
            IYahooPageLoader pageLoader,
            IYahooJsonNavigator jsonNavigator,
            ILogger<YahooNewsScraper> logger)
        {
            _pageLoader = pageLoader;
            _jsonNavigator = jsonNavigator;
            _logger = logger;
        }

        public async Task<List<YahooNewsItemDto>?> GetNewsAsync(string symbol, int? limit = null)
        {
            if (!TryNormalizeSymbol(symbol, out var normalizedSymbol))
                return null;

            var pageContext = await _pageLoader.LoadNewsPageAsync(normalizedSymbol);
            if (pageContext is null)
                return null;

            _logger.LogInformation("Scraping Yahoo news for {Symbol}", normalizedSymbol);

            var payloadRoots = pageContext.PayloadRoots;
            var news = ParseNewsFromJson(payloadRoots, normalizedSymbol);

            if (news.Count == 0)
                news = ParseNewsFromHtml(pageContext.Document);

            if (limit is > 0)
                news = news.Take(limit.Value).ToList();

            return news.Count > 0 ? news : null;
        }

        private List<YahooNewsItemDto> ParseNewsFromJson(IReadOnlyList<JsonElement> payloadRoots, string symbol)
        {
            var result = new List<YahooNewsItemDto>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var root in payloadRoots)
            {
                foreach (var current in _jsonNavigator.Traverse(root))
                {
                    if (current.ValueKind != JsonValueKind.Object)
                        continue;

                    // We expect objects like:
                    // {
                    //   "content": { "contentType":"STORY", "title":"...", "summary":"...", "pubDate":"..." },
                    //   "canonicalUrl": { "url":"..." },
                    //   "clickThroughUrl": { "url":"..." },
                    //   "finance": { "stockTickers":[{"symbol":"MVST"}] }
                    // }

                    if (!current.TryGetProperty("content", out var contentNode) ||
                        contentNode.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    if (!contentNode.TryGetProperty("contentType", out var contentTypeNode) ||
                        contentTypeNode.ValueKind != JsonValueKind.String ||
                        !string.Equals(contentTypeNode.GetString(), "STORY", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Filter to requested ticker when available
                    if (!current.TryGetProperty("finance", out var financeNode) ||
                        financeNode.ValueKind != JsonValueKind.Object ||
                        !financeNode.TryGetProperty("stockTickers", out var tickersNode) ||
                        tickersNode.ValueKind != JsonValueKind.Array ||
                        tickersNode.GetArrayLength() == 0)
                    {
                        continue;
                    }
                    if (!ContainsSymbol(tickersNode, symbol))
                    {
                        continue;
                    }

                    if (!contentNode.TryGetProperty("title", out var titleNode) ||
                        titleNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }
                    var title = YahooHtmlValueReader.Clean(titleNode.GetString());
                    if (string.IsNullOrWhiteSpace(title))
                        continue;

                    if (!contentNode.TryGetProperty("summary", out var summaryNode) ||
                        summaryNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }
                    var summary = YahooHtmlValueReader.Clean(summaryNode.GetString());

                    if (!contentNode.TryGetProperty("displayTime", out var displayTimeNode) ||
                        displayTimeNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }
                    var publishTime = YahooHtmlValueReader.Clean(displayTimeNode.GetString());

                    if (!current.TryGetProperty("canonicalUrl", out var canonicalUrlNode) ||
                        canonicalUrlNode.ValueKind != JsonValueKind.Object ||
                        !canonicalUrlNode.TryGetProperty("url", out var canonicalUrlValueNode) ||
                        canonicalUrlValueNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }
                    var url = YahooHtmlValueReader.Clean(canonicalUrlValueNode.GetString());

                    if (!current.TryGetProperty("clickThroughUrl", out var clickThroughUrlNode) ||
                        clickThroughUrlNode.ValueKind != JsonValueKind.Object ||
                        !clickThroughUrlNode.TryGetProperty("url", out var clickThroughUrlValueNode) ||
                        clickThroughUrlValueNode.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }
                    var clickThroughUrl = YahooHtmlValueReader.Clean(clickThroughUrlValueNode.GetString());

                    if (!clickThroughUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        clickThroughUrl = "https://finance.yahoo.com" + clickThroughUrl;

                    var dedupeKey = $"{title}|{url}";
                    if (!seen.Add(dedupeKey))
                        continue;

                    result.Add(new YahooNewsItemDto
                    {
                        Title = title,
                        Url = clickThroughUrl,
                        Summary = string.IsNullOrWhiteSpace(summary) ? "N/A" : summary,
                        PublishTime = string.IsNullOrWhiteSpace(publishTime) ? "N/A" : publishTime
                    });
                }
            }

            return result;
        }

        private static List<YahooNewsItemDto> ParseNewsFromHtml(HtmlDocument document)
        {
            var result = new List<YahooNewsItemDto>();
            var newsNodes = document.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");
            if (newsNodes is null)
                return result;

            foreach (var node in newsNodes)
            {
                try
                {
                    var titleNode = node.SelectSingleNode(".//h3");
                    var urlNode = node.SelectSingleNode(".//a[contains(@class,'subtle-link') and @href]");
                    var summaryNode = node.SelectSingleNode(".//p");
                    var timeNode = node.SelectSingleNode(".//div[contains(@class, 'publishing')]");

                    var title = YahooHtmlValueReader.Clean(titleNode?.InnerText);
                    if (string.IsNullOrWhiteSpace(title))
                        continue;

                    var url = YahooHtmlValueReader.Clean(urlNode?.GetAttributeValue("href", string.Empty));
                    if (string.IsNullOrWhiteSpace(url))
                        continue;

                    if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        url = "https://finance.yahoo.com" + url;

                    var summary = YahooHtmlValueReader.Clean(summaryNode?.InnerText);
                    var publishTime = YahooHtmlValueReader.Clean(timeNode?.InnerText);

                    if (!string.IsNullOrWhiteSpace(publishTime) && publishTime.Contains('•'))
                        publishTime =
                            publishTime.Split('•',
                                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                .LastOrDefault() ?? publishTime;

                    result.Add(new YahooNewsItemDto
                    {
                        Title = title,
                        Url = url,
                        Summary = string.IsNullOrWhiteSpace(summary) ? "N/A" : summary,
                        PublishTime = string.IsNullOrWhiteSpace(publishTime) ? "N/A" : publishTime
                    });
                }
                catch
                {
                    // Ignore broken story blocks
                }
            }

            return result;
        }

        private static bool ContainsSymbol(JsonElement tickersNode, string symbol)
        {
            foreach (var ticker in tickersNode.EnumerateArray())
            {
                if (ticker.ValueKind != JsonValueKind.Object ||
                    !ticker.TryGetProperty("symbol", out var tickerSymbolNode) ||
                    tickerSymbolNode.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                if (string.Equals(tickerSymbolNode.GetString(), symbol, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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
}
