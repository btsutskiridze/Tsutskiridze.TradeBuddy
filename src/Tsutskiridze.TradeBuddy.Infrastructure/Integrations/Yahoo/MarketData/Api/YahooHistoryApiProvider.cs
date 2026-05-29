using System.Net.Http.Json;
using System.Text.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;

public class YahooHistoryApiProvider : IYahooHistoryApiProvider
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public YahooHistoryApiProvider(
        HttpClient httpClient,
        InfraJsonSerializerOptions jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions.Options;
    }

    public async Task<IReadOnlyList<MarketCandle>> GetDailyCandles(string symbol, DateOnly from, DateOnly to,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        if (from > to)
            throw new ArgumentException("'from' date cannot be greater than 'to' date.");

        var requestUri = BuildChartUri(symbol, from, to);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"Yahoo chart request failed. StatusCode: {(int)response.StatusCode}. Body: {body}");
        }

        var yahooResponse = await response.Content.ReadFromJsonAsync<YahooChartResponse>(
            _jsonSerializerOptions,
            ct);

        if (yahooResponse is null)
            throw new InvalidOperationException("Yahoo returned empty response.");

        return MapToCandles(yahooResponse, symbol, from, to);
    }

    public async Task<MarketHistoryDateRange> GetClosedDailyDateRange(
        string symbol,
        DateTimeOffset nowUtc,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        var requestUri = BuildChartMetadataUri(symbol);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"Yahoo chart metadata request failed. StatusCode: {(int)response.StatusCode}. Body: {body}");
        }

        var yahooResponse = await response.Content.ReadFromJsonAsync<YahooChartResponse>(
            _jsonSerializerOptions,
            ct);

        if (yahooResponse is null)
            throw new InvalidOperationException("Yahoo returned empty response.");

        return MapToClosedDailyDateRange(yahooResponse, symbol, nowUtc);
    }

    private static string BuildChartUri(string symbol, DateOnly from, DateOnly to)
    {
        var normalizedSymbol = Uri.EscapeDataString(symbol.Trim().ToUpperInvariant());
        var period1 = ToUnixSeconds(from);
        // period2 is better treated as exclusive.
        // Add one day to include the requested 'to' date.
        var period2 = ToUnixSeconds(to.AddDays(1));

        return $"v8/finance/chart/{normalizedSymbol}" +
               $"?period1={period1}" +
               $"&period2={period2}" +
               "&interval=1d" +
               "&includePrePost=false" +
               "&events=div|split";
    }
    
    private static string BuildChartMetadataUri(string symbol)
    {
        var normalizedSymbol = Uri.EscapeDataString(symbol.Trim().ToUpperInvariant());

        return $"v8/finance/chart/{normalizedSymbol}" +
               "?range=1d" +
               "&interval=1d" +
               "&includePrePost=false" +
               "&events=div|split";
    }
    
    private static MarketHistoryDateRange MapToClosedDailyDateRange(
        YahooChartResponse response,
        string requestedSymbol,
        DateTimeOffset nowUtc)
    {
        var chart = response.Chart
                    ?? throw new InvalidOperationException("Yahoo response does not contain chart node.");

        if (chart.Error is not null)
        {
            throw new InvalidOperationException(
                $"Yahoo returned chart error. Code: {chart.Error.Code}. Description: {chart.Error.Description}");
        }

        var result = chart.Result?.FirstOrDefault();

        if (result is null)
            throw new InvalidOperationException("Yahoo response does not contain chart result.");

        if (!string.IsNullOrWhiteSpace(result.Meta?.Symbol) &&
            !string.Equals(result.Meta.Symbol, requestedSymbol, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Yahoo returned symbol '{result.Meta.Symbol}', but requested '{requestedSymbol}'.");
        }

        var exchangeTimeZone = TryGetExchangeTimeZone(result.Meta?.ExchangeTimezoneName)
                               ?? TimeZoneInfo.Utc;

        var marketNow = TimeZoneInfo.ConvertTime(nowUtc, exchangeTimeZone);
        var marketToday = DateOnly.FromDateTime(marketNow.DateTime);

        var regularMarketEndUtc = result.Meta?.CurrentTradingPeriod?.Regular?.End is long end
            ? DateTimeOffset.FromUnixTimeSeconds(end)
            : (DateTimeOffset?)null;

        DateOnly to;

        if (regularMarketEndUtc is not null)
        {
            var marketAlreadyClosed = nowUtc >= regularMarketEndUtc.Value;

            to = marketAlreadyClosed
                ? marketToday
                : PreviousTradingWeekday(marketToday.AddDays(-1));
        }
        else
        {
            // Fallback when Yahoo metadata is missing.
            // Better than crashing, but less accurate because we do not know exact market close.
            to = PreviousTradingWeekday(marketToday.AddDays(-1));
        }

        //todo: make the date to be passed to  amethod
        return new MarketHistoryDateRange(
            From: to.AddYears(-1),
            To: to);
    }
    
    private static DateOnly PreviousTradingWeekday(DateOnly date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(-1);
        }

        return date;
    }

    private static long ToUnixSeconds(DateOnly date)
    {
        var utcDateTime = DateTime.SpecifyKind(
            date.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);

        return new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
    }
    
    private static IReadOnlyList<MarketCandle> MapToCandles(
        YahooChartResponse response,
        string requestedSymbol,
        DateOnly from,
        DateOnly to)
    {
        var chart = response.Chart
            ?? throw new InvalidOperationException("Yahoo response does not contain chart node.");

        if (chart.Error is not null)
        {
            throw new InvalidOperationException(
                $"Yahoo returned chart error. Code: {chart.Error.Code}. Description: {chart.Error.Description}");
        }

        var result = chart.Result?.FirstOrDefault();

        if (result is null)
            return [];

        if (!string.IsNullOrWhiteSpace(result.Meta?.Symbol) &&
            !string.Equals(result.Meta.Symbol, requestedSymbol, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Yahoo returned symbol '{result.Meta.Symbol}', but requested '{requestedSymbol}'.");
        }

        var timestamps = result.Timestamp;
        var quote = result.Indicators?.Quote?.FirstOrDefault();

        if (timestamps is null || quote is null)
            return [];

        if (quote.Open is null ||
            quote.High is null ||
            quote.Low is null ||
            quote.Close is null ||
            quote.Volume is null)
        {
            return [];
        }

        var count = new[]
        {
            timestamps.Length,
            quote.Open.Length,
            quote.High.Length,
            quote.Low.Length,
            quote.Close.Length,
            quote.Volume.Length
        }.Min();

        var exchangeTimeZone = TryGetExchangeTimeZone(result.Meta?.ExchangeTimezoneName);

        var candles = new List<MarketCandle>(count);

        for (var i = 0; i < count; i++)
        {
            var open = quote.Open[i];
            var high = quote.High[i];
            var low = quote.Low[i];
            var close = quote.Close[i];

            if (open is null || high is null || low is null || close is null)
                continue;

            var date = ToExchangeDate(timestamps[i], exchangeTimeZone);

            if (date < from || date > to)
                continue;

            candles.Add(new MarketCandle(
                Date: date,
                Open: open.Value,
                High: high.Value,
                Low: low.Value,
                Close: close.Value,
                Volume: quote.Volume[i] ?? 0));
        }

        return candles
            .OrderBy(x => x.Date)
            .ToArray();
    }

    private static TimeZoneInfo? TryGetExchangeTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return null;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return null;
        }
    }

    private static DateOnly ToExchangeDate(long unixSeconds, TimeZoneInfo? exchangeTimeZone)
    {
        var utc = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

        if (exchangeTimeZone is null)
            return DateOnly.FromDateTime(utc.UtcDateTime);

        var exchangeTime = TimeZoneInfo.ConvertTime(utc, exchangeTimeZone);

        return DateOnly.FromDateTime(exchangeTime.DateTime);
    }
}