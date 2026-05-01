using System.Text.Json;
using HtmlAgilityPack;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;

public interface IYahooPayloadExtractor
{
    IReadOnlyList<JsonElement> ExtractPayloadRoots(HtmlDocument document);
}

