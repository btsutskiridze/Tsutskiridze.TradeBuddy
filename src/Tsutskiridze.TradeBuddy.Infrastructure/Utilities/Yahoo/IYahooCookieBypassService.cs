using Microsoft.Extensions.Logging;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Utilities.Yahoo
{
    public interface IYahooCookieBypassService
    {
        Task<string> GetHtmlContentWithCookieBypass(HttpClient client, string url, ILogger logger);
    }
}
