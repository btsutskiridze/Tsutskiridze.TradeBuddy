using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo
{
    public class YahooScraperBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger<YahooScraperBase> _logger;
        protected readonly IYahooCookieBypassService _yahooCookieBypassService;

        public YahooScraperBase(HttpClient httpClient, ILogger<YahooScraperBase> logger, IYahooCookieBypassService yahooCookieBypassService)
        {
            _yahooCookieBypassService = yahooCookieBypassService;
            _httpClient = httpClient;
            _logger = logger;
        }

        protected async Task<HtmlDocument> GetHtmlDocumentAsync(string relativeUrl)
        {
            // Compose the full URL including language and region parameters.
            string url = $"{relativeUrl}?lang=en-US&region=US";
            string html = await _yahooCookieBypassService.GetHtmlContentWithCookieBypass(_httpClient, url, _logger);
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}
