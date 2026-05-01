using System.Text.Json;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Abstractions;

public interface IYahooJsonNavigator
{
    IEnumerable<JsonElement> Traverse(JsonElement root);
    JsonElement? FindObjectByPropertyName(JsonElement root, string propertyName);
    JsonElement? FindArrayByPropertyName(JsonElement root, string propertyName);
    JsonElement? FindQuoteObjectBySymbol(JsonElement root, string symbol);
    JsonElement? FindFirstObject(IEnumerable<JsonElement> roots, string propertyName);
    JsonElement? FindFirstArray(IEnumerable<JsonElement> roots, string propertyName);
    JsonElement? FindFirstQuoteObject(IEnumerable<JsonElement> roots, string symbol);
    string? FindFirstEarningsDate(IEnumerable<JsonElement> roots);
    string? TryGetString(JsonElement element, string propertyName);
    string? TryGetFmtOrRawString(JsonElement element, string propertyName);
    double? TryGetDoubleAt(JsonElement arrayNode, int index);
    long? TryGetLongAt(JsonElement arrayNode, int index);
}

