using System.Text.Json;
using HtmlAgilityPack;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;

public interface IYahooPayloadExtractor
{
    IReadOnlyList<JsonElement> ExtractPayloadRoots(HtmlDocument document);
}

