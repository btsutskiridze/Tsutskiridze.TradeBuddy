using HtmlAgilityPack;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;

namespace Tsutskiridze.TradeBuddy.Services.News.Yahoo
{
    public class YahooSraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<YahooSraper> _logger;

        public YahooSraper(HttpClient httpClient, ILogger<YahooSraper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<YahooNews>?> GetNewsAsync(string symbol, int? limit = null)
        {
            var response = await _httpClient.GetAsync($"quote/{symbol}/news");
            response.EnsureSuccessStatusCode();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

            _logger.LogInformation("Scraping Yahoo news for {Symbol}", symbol);
            _logger.LogInformation("HTML: {Html}", await response.Content.ReadAsStringAsync());

            var newsNodes = htmlDocument.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");

            if (newsNodes == null)
            {
                return null;
            }

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
                    if (!newsUrl.StartsWith("https")) newsUrl = "https://finance.yahoo.com" + newsUrl;
                    string summary = summaryNode?.InnerText.Trim() ?? "N/A";
                    string publishTime = timeNode?.InnerText.Trim() ?? "N/A";

                    newsList.Add(new YahooNews
                    {
                        Title = title,
                        Url = newsUrl,
                        Summary = summary,
                        PublishTime = publishTime
                    });
                }
                catch (Exception)
                {
                }

                if (newsList.Count >= limit)
                {
                    break;
                }
            }

            return newsList.Count > 0 ? newsList : null;
        }
    }
}

