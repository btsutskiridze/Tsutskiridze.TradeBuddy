using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;

internal sealed class YahooChartResponse
{
    [JsonPropertyName("chart")]
    public YahooChart? Chart { get; init; }
}

internal sealed class YahooChart
{
    [JsonPropertyName("result")]
    public List<YahooChartResult>? Result { get; init; }

    [JsonPropertyName("error")]
    public YahooChartError? Error { get; init; }
}

internal sealed class YahooChartError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

internal sealed class YahooChartResult
{
    [JsonPropertyName("meta")]
    public YahooChartMeta? Meta { get; init; }

    [JsonPropertyName("timestamp")]
    public long[]? Timestamp { get; init; }

    [JsonPropertyName("indicators")]
    public YahooChartIndicators? Indicators { get; init; }
}

internal sealed class YahooChartMeta
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("exchangeTimezoneName")]
    public string? ExchangeTimezoneName { get; init; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
}

internal sealed class YahooChartIndicators
{
    [JsonPropertyName("quote")]
    public YahooChartQuote[]? Quote { get; init; }

    [JsonPropertyName("adjclose")]
    public YahooChartAdjClose[]? AdjClose { get; init; }
}

internal sealed class YahooChartQuote
{
    [JsonPropertyName("open")]
    public decimal?[]? Open { get; init; }

    [JsonPropertyName("high")]
    public decimal?[]? High { get; init; }

    [JsonPropertyName("low")]
    public decimal?[]? Low { get; init; }

    [JsonPropertyName("close")]
    public decimal?[]? Close { get; init; }

    [JsonPropertyName("volume")]
    public long?[]? Volume { get; init; }
}

internal sealed class YahooChartAdjClose
{
    [JsonPropertyName("adjclose")]
    public decimal?[]? AdjClose { get; init; }
}