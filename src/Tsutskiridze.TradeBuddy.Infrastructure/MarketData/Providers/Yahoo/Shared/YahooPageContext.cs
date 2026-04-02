using System.Text.Json;
using HtmlAgilityPack;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared;

public sealed record YahooPageContext(
    string Symbol,
    HtmlDocument Document,
    IReadOnlyList<JsonElement> PayloadRoots);

