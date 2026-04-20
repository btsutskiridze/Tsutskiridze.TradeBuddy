using System.Text.Json;
using HtmlAgilityPack;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;

public sealed record YahooPageContext(
    string Symbol,
    HtmlDocument Document,
    IReadOnlyList<JsonElement> PayloadRoots);

