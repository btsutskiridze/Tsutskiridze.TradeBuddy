using HtmlAgilityPack;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;

namespace Tsutskiridze.TradeBuddy.Services.News
{
    public class YahooSraper
    {
        private readonly HttpClient _httpClient;

        public YahooSraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<YahooNews>> GetNewsAsync(string symbol)
        {
            var response = await _httpClient.GetAsync($"quote/{symbol}/news");
            response.EnsureSuccessStatusCode();

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(await response.Content.ReadAsStringAsync());

            var newsList = new List<YahooNews>();

            var newsNodes = htmlDocument.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");
            if (newsNodes != null)
            {
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
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing article: " + ex.Message);
                    }
                }
            }
            return newsList;
        }
    }
}

