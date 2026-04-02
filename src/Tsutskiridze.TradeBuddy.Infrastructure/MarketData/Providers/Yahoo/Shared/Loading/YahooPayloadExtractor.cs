using System.Text.Json;
using HtmlAgilityPack;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Helpers;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Loading;

public class YahooPayloadExtractor : IYahooPayloadExtractor
{
    public IReadOnlyList<JsonElement> ExtractPayloadRoots(HtmlDocument document)
    {
        var roots = new List<JsonElement>();
        var scriptNodes = document.DocumentNode.SelectNodes("//script[@type='application/json']");

        if (scriptNodes is null)
            return roots;

        foreach (var scriptNode in scriptNodes)
        {
            var rawScript = YahooHtmlValueReader.Clean(scriptNode.InnerText);
            if (string.IsNullOrWhiteSpace(rawScript))
                continue;

            try
            {
                using var outerDocument = JsonDocument.Parse(rawScript);

                if (outerDocument.RootElement.ValueKind == JsonValueKind.Object &&
                    outerDocument.RootElement.TryGetProperty("body", out var bodyNode) &&
                    bodyNode.ValueKind == JsonValueKind.String)
                {
                    var body = bodyNode.GetString();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            using var payloadDocument = JsonDocument.Parse(body);
                            roots.Add(payloadDocument.RootElement.Clone());
                            continue;
                        }
                        catch
                        {
                            // Fall back to outer JSON below.
                        }
                    }
                }

                roots.Add(outerDocument.RootElement.Clone());
            }
            catch
            {
                // Ignore unrelated or malformed JSON blocks.
            }
        }

        return roots;
    }
}

