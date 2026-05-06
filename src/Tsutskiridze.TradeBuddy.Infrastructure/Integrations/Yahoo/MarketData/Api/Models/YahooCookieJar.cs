using System.Net;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;


internal sealed class YahooCookieJar
{
    public CookieContainer CookieContainer { get; } = new();
}