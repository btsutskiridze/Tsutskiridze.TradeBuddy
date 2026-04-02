using System.Globalization;
using System.Text.Json;
using Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Helpers;

public class YahooJsonNavigator : IYahooJsonNavigator
{
    public IEnumerable<JsonElement> Traverse(JsonElement root)
    {
        var stack = new Stack<JsonElement>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            if (current.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in current.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object || property.Value.ValueKind == JsonValueKind.Array)
                        stack.Push(property.Value);
                }
            }
            else if (current.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in current.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                        stack.Push(item);
                }
            }
        }
    }

    public JsonElement? FindObjectByPropertyName(JsonElement root, string propertyName)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var property in current.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Object)
                    return property.Value.Clone();
            }
        }

        return null;
    }

    public JsonElement? FindArrayByPropertyName(JsonElement root, string propertyName)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var property in current.EnumerateObject())
            {
                if (property.NameEquals(propertyName) && property.Value.ValueKind == JsonValueKind.Array)
                    return property.Value.Clone();
            }
        }

        return null;
    }

    public JsonElement? FindQuoteObjectBySymbol(JsonElement root, string symbol)
    {
        foreach (var current in Traverse(root))
        {
            if (current.ValueKind != JsonValueKind.Object)
                continue;

            if (!current.TryGetProperty("symbol", out var symbolNode) ||
                symbolNode.ValueKind != JsonValueKind.String ||
                !string.Equals(symbolNode.GetString(), symbol, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (current.TryGetProperty("regularMarketPrice", out _) ||
                current.TryGetProperty("shortName", out _) ||
                current.TryGetProperty("longName", out _) ||
                current.TryGetProperty("quoteType", out _))
            {
                return current.Clone();
            }
        }

        return null;
    }

    public JsonElement? FindFirstObject(IEnumerable<JsonElement> roots, string propertyName)
    {
        foreach (var root in roots)
        {
            var match = FindObjectByPropertyName(root, propertyName);
            if (match is not null)
                return match;
        }

        return null;
    }

    public JsonElement? FindFirstArray(IEnumerable<JsonElement> roots, string propertyName)
    {
        foreach (var root in roots)
        {
            var match = FindArrayByPropertyName(root, propertyName);
            if (match is not null)
                return match;
        }

        return null;
    }

    public JsonElement? FindFirstQuoteObject(IEnumerable<JsonElement> roots, string symbol)
    {
        foreach (var root in roots)
        {
            var match = FindQuoteObjectBySymbol(root, symbol);
            if (match is not null)
                return match;
        }

        return null;
    }

    public string? FindFirstEarningsDate(IEnumerable<JsonElement> roots)
    {
        foreach (var root in roots)
        {
            var calendarEvents = FindObjectByPropertyName(root, "calendarEvents");
            if (calendarEvents is null)
                continue;

            if (!calendarEvents.Value.TryGetProperty("earnings", out var earningsNode) ||
                earningsNode.ValueKind != JsonValueKind.Object ||
                !earningsNode.TryGetProperty("earningsDate", out var earningsDateNode) ||
                earningsDateNode.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in earningsDateNode.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object &&
                    item.TryGetProperty("fmt", out var fmtNode) &&
                    fmtNode.ValueKind == JsonValueKind.String)
                {
                    return fmtNode.GetString();
                }
            }
        }

        return null;
    }

    public string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var node))
            return null;

        return node.ValueKind switch
        {
            JsonValueKind.String => node.GetString(),
            JsonValueKind.Number => node.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    public string? TryGetFmtOrRawString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var node))
            return null;

        if (node.ValueKind == JsonValueKind.Object)
        {
            if (node.TryGetProperty("longFmt", out var longFmtNode) && longFmtNode.ValueKind == JsonValueKind.String)
                return longFmtNode.GetString();

            if (node.TryGetProperty("fmt", out var fmtNode) && fmtNode.ValueKind == JsonValueKind.String)
                return fmtNode.GetString();

            if (node.TryGetProperty("raw", out var rawNode))
            {
                if (rawNode.ValueKind == JsonValueKind.String)
                    return rawNode.GetString();

                if (rawNode.ValueKind == JsonValueKind.Number)
                    return rawNode.GetRawText();
            }
        }

        if (node.ValueKind == JsonValueKind.String)
            return node.GetString();

        if (node.ValueKind == JsonValueKind.Number)
            return node.GetRawText();

        return null;
    }

    public double? TryGetDoubleAt(JsonElement arrayNode, int index)
    {
        if (arrayNode.ValueKind != JsonValueKind.Array || index < 0 || index >= arrayNode.GetArrayLength())
            return null;

        var item = arrayNode[index];

        if (item.ValueKind == JsonValueKind.Number && item.TryGetDouble(out var value))
            return value;

        if (item.ValueKind == JsonValueKind.String &&
            double.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    public long? TryGetLongAt(JsonElement arrayNode, int index)
    {
        if (arrayNode.ValueKind != JsonValueKind.Array || index < 0 || index >= arrayNode.GetArrayLength())
            return null;

        var item = arrayNode[index];

        if (item.ValueKind == JsonValueKind.Number && item.TryGetInt64(out var value))
            return value;

        if (item.ValueKind == JsonValueKind.String &&
            long.TryParse(item.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
    }
}

