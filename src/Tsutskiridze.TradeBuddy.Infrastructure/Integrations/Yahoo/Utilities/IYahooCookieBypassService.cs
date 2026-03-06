using Microsoft.Extensions.Logging;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Utilities
{
    public interface IYahooCookieBypassService
    {
        Task<string> GetHtmlContentWithCookieBypass(HttpClient client, string url, ILogger logger);
    }
}
