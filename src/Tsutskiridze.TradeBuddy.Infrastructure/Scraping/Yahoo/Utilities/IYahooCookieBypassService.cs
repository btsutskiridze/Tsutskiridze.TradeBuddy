using Microsoft.Extensions.Logging;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Scraping.Yahoo.Utilities
{
    public interface IYahooCookieBypassService
    {
        Task<string> GetHtmlContentWithCookieBypass(HttpClient client, string url, ILogger logger);
    }
}
