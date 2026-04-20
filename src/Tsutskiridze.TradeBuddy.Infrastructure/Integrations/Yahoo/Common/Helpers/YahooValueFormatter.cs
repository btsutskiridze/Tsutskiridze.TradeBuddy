using System.Globalization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Helpers;

public static class YahooValueFormatter
{
    public static string FormatNumber(double? value) =>
        value?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;

    public static string FormatVolume(long? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    public static string RemoveCommas(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace(",", string.Empty);

    public static (string Low, string High) SplitRange(string rangeText)
    {
        if (string.IsNullOrWhiteSpace(rangeText))
            return (string.Empty, string.Empty);

        var parts = rangeText.Split(" - ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : (string.Empty, string.Empty);
    }

    public static string PreferExisting(string? currentValue, params string?[] fallbacks)
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
            return currentValue;

        foreach (var fallback in fallbacks)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback;
        }

        return string.Empty;
    }
}

