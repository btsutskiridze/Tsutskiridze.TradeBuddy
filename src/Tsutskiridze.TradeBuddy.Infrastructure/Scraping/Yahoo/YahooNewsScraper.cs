using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Dtos.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo
{
    public class YahooNewsScraper : YahooScraperBase, IYahooNewsProvider
    {
        public YahooNewsScraper(
            HttpClient httpClient,
            ILogger<YahooScraperBase> logger,
            IYahooCookieBypassService yahooCookieBypassService
        ) : base(httpClient, logger, yahooCookieBypassService)
        {
        }

        public async Task<List<YahooNews>?> GetNewsAsync(string symbol, int? limit = null)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}/news");

            var newsNodes = document.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");
            if (newsNodes == null)
            {
                return null;
            }

            _logger.LogInformation("Scraping Yahoo news for {Symbol}", symbol);
            var newsList = new List<YahooNews>();

            foreach (var node in newsNodes)
            {
                try
                {
                    var titleNode = node.SelectSingleNode(".//h3");
                    var urlNode = node.SelectSingleNode(".//a[@class='subtle-link']");
                    var summaryNode = node.SelectSingleNode(".//p");
                    var timeNode = node.SelectSingleNode(".//div[contains(@class, 'publishing')]");

                    string title = titleNode?.InnerText.Trim() ?? "N/A";
                    string newsUrl = urlNode?.GetAttributeValue("href", "").Trim() ?? "N/A";
                    if (!newsUrl.StartsWith("https"))
                        newsUrl = "https://finance.yahoo.com" + newsUrl;
                    string summary = summaryNode?.InnerText.Trim() ?? "N/A";
                    string publishTime = timeNode?.InnerText.Trim().Split("• ").Last() ?? "N/A";

                    newsList.Add(new YahooNews
                    {
                        Title = title,
                        Url = newsUrl,
                        Summary = summary,
                        PublishTime = publishTime
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing a news item.");
                }

                if (limit.HasValue && newsList.Count >= limit.Value)
                {
                    break;
                }
            }

            return newsList.Count > 0 ? newsList : null;
        }

    }
}
