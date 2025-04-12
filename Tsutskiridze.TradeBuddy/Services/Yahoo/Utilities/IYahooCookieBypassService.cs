namespace Tsutskiridze.TradeBuddy.Services.Yahoo.Utilities
{
    public interface IYahooCookieBypassService
    {
        Task<string> GetHtmlContentWithCookieBypass(HttpClient client, string url, ILogger logger);
    }
}
