using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;

internal sealed class YahooQuoteApiResponse
{
    [JsonPropertyName("quoteResponse")]
    public YahooQuoteResponseBody? QuoteResponse { get; init; }
}

internal sealed class YahooQuoteResponseBody
{
    [JsonPropertyName("result")]
    public YahooQuoteResult[]? Result { get; init; }

    [JsonPropertyName("error")]
    public JsonElement? Error { get; init; }
}

internal sealed class YahooQuoteResult
{
    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; init; }

    [JsonPropertyName("longName")]
    public string? LongName { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("regularMarketPrice")]
    public decimal? RegularMarketPrice { get; init; }

    [JsonPropertyName("regularMarketChangePercent")]
    public decimal? RegularMarketChangePercent { get; init; }

    [JsonPropertyName("regularMarketChange")]
    public decimal? RegularMarketChange { get; init; }

    [JsonPropertyName("regularMarketDayLow")]
    public decimal? RegularMarketDayLow { get; init; }

    [JsonPropertyName("regularMarketDayHigh")]
    public decimal? RegularMarketDayHigh { get; init; }

    [JsonPropertyName("fiftyTwoWeekHigh")]
    public decimal? FiftyTwoWeekHigh { get; init; }

    [JsonPropertyName("fiftyTwoWeekLow")]
    public decimal? FiftyTwoWeekLow { get; init; }

    [JsonPropertyName("marketCap")]
    public long? MarketCap { get; init; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; init; }

    [JsonPropertyName("fullExchangeName")]
    public string? FullExchangeName { get; init; }

    [JsonPropertyName("regularMarketVolume")]
    public long? RegularMarketVolume { get; init; }

    [JsonPropertyName("averageDailyVolume3Month")]
    public long? AverageDailyVolume3Month { get; init; }

    [JsonPropertyName("averageDailyVolume10Day")]
    public long? AverageDailyVolume10Day { get; init; }

    [JsonPropertyName("regularMarketOpen")]
    public decimal? RegularMarketOpen { get; init; }

    [JsonPropertyName("regularMarketPreviousClose")]
    public decimal? RegularMarketPreviousClose { get; init; }

    [JsonPropertyName("epsTrailingTwelveMonths")]
    public decimal? EpsTrailingTwelveMonths { get; init; }

    [JsonPropertyName("trailingPE")]
    public decimal? TrailingPE { get; init; }

    [JsonPropertyName("forwardPE")]
    public decimal? ForwardPE { get; init; }

    [JsonPropertyName("earningsTimestamp")]
    public long? EarningsTimestamp { get; init; }

    [JsonPropertyName("regularMarketTime")]
    public long? RegularMarketTime { get; init; }
}