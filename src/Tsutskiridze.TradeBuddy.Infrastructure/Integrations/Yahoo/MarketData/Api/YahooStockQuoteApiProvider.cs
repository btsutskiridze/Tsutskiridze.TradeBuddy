using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api;

internal class YahooStockQuoteApiProvider : IYahooStockQuoteApiProvider
{
    private static readonly JsonSerializerOptions YahooJsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly TimeSpan YahooCrumbLifetime = TimeSpan.FromHours(6);
    
    private readonly YahooCrumbCache _crumbCache;
    private readonly HttpClient _httpClient;

    public YahooStockQuoteApiProvider(HttpClient httpClient, YahooCrumbCache crumbCache)
    {
        _httpClient = httpClient;
        _crumbCache = crumbCache;
    }

    public async Task<StockQuote?> GetStockQuote(string symbol, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        try
        {
            // First attempt uses cached crumb if available.
            var quote = await TryGetStockQuote(
                normalizedSymbol,
                forceRefreshCrumb: false,
                ct);

            if (quote is not null)
                return quote;

            // Second attempt forces fresh Yahoo cookies + crumb.
            return await TryGetStockQuote(
                normalizedSymbol,
                forceRefreshCrumb: true,
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            // Important: return null so MarketDataProvider can fallback to scraping.
            _crumbCache.Session = null;
            return null;
        }
    }

    private async Task<StockQuote?> TryGetStockQuote(
        string symbol,
        bool forceRefreshCrumb,
        CancellationToken ct)
    {
        var crumb = await GetYahooCrumb(symbol, forceRefreshCrumb, ct);

        var requestUri = BuildYahooQuoteUri(symbol, crumb);

        using var request = CreateYahooRequest(requestUri);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            _crumbCache.Session = null;
            return null;
        }

        if (!response.IsSuccessStatusCode)
            return null;

        var yahooResponse = await response.Content.ReadFromJsonAsync<YahooQuoteApiResponse>(
            YahooJsonOptions,
            ct);

        if (yahooResponse?.QuoteResponse?.Result is null ||
            yahooResponse.QuoteResponse.Result.Length == 0)
        {
            return null;
        }

        var result = yahooResponse.QuoteResponse.Result.First();

        if (!string.Equals(result.Symbol, symbol, StringComparison.OrdinalIgnoreCase))
            return null;

        return MapToStockQuote(result, symbol);
    }

    private async Task<string> GetYahooCrumb(
        string symbol,
        bool forceRefresh,
        CancellationToken ct)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var session = _crumbCache.Session;

        if (!forceRefresh && session is not null && session.ExpiresAtUtc > nowUtc)
            return session.Crumb;

        await _crumbCache.WaitAsync(ct);

        try
        {
            nowUtc = DateTimeOffset.UtcNow;
            session = _crumbCache.Session;

            if (!forceRefresh && session is not null && session.ExpiresAtUtc > nowUtc)
                return session.Crumb;

            await PrimeYahooCookies(symbol, ct);

            using var request = CreateYahooRequest(
                new Uri("https://query2.finance.yahoo.com/v1/test/getcrumb"));

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);

                throw new HttpRequestException(
                    $"Yahoo crumb request failed. StatusCode: {(int)response.StatusCode}. Body: {body}");
            }

            var crumb = (await response.Content.ReadAsStringAsync(ct)).Trim();

            if (string.IsNullOrWhiteSpace(crumb) || crumb.Contains('<'))
                throw new InvalidOperationException("Yahoo returned invalid crumb.");

            _crumbCache.Session =
                new YahooCrumbCache.YahooCrumbSession(
                    crumb,
                    DateTimeOffset.UtcNow.Add(YahooCrumbLifetime)
                );

            return crumb;
        }
        finally
        {
            _crumbCache.Release();
        }
    }

    private async Task PrimeYahooCookies(string symbol, CancellationToken ct)
    {
        var quotePageUri = new Uri(
            $"https://finance.yahoo.com/quote/{Uri.EscapeDataString(symbol)}");

        using var request = CreateYahooRequest(quotePageUri);

        request.Headers.Accept.Clear();
        request.Headers.Accept.ParseAdd(
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            throw new HttpRequestException(
                $"Yahoo quote page request failed. StatusCode: {(int)response.StatusCode}. Body: {body}");
        }
    }

    private static Uri BuildYahooQuoteUri(string symbol, string crumb)
    {
        var encodedSymbol = Uri.EscapeDataString(symbol);
        var encodedCrumb = Uri.EscapeDataString(crumb);

        return new Uri(
            $"https://query2.finance.yahoo.com/v7/finance/quote" +
            $"?symbols={encodedSymbol}" +
            $"&crumb={encodedCrumb}");
    }

    private static HttpRequestMessage CreateYahooRequest(Uri uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/124 Safari/537.36");

        request.Headers.Accept.ParseAdd("application/json,text/plain,*/*");
        request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
        request.Headers.Referrer = new Uri("https://finance.yahoo.com/");

        return request;
    }

    private static StockQuote MapToStockQuote(
        YahooQuoteResult quote,
        string requestedSymbol)
    {
        return new StockQuote
        {
            Symbol = quote.Symbol ?? requestedSymbol,
            Name = quote.LongName
                   ?? quote.ShortName
                   ?? quote.DisplayName
                   ?? string.Empty,

            Currency = quote.Currency ?? string.Empty,

            Price = FormatDecimal(quote.RegularMarketPrice),
            ChangesPercentage = FormatPercentage(quote.RegularMarketChangePercent),
            Change = FormatDecimal(quote.RegularMarketChange),

            DayLow = FormatDecimal(quote.RegularMarketDayLow),
            DayHigh = FormatDecimal(quote.RegularMarketDayHigh),

            YearHigh = FormatDecimal(quote.FiftyTwoWeekHigh),
            YearLow = FormatDecimal(quote.FiftyTwoWeekLow),

            MarketCap = FormatLong(quote.MarketCap),

            Exchange = quote.FullExchangeName
                       ?? quote.Exchange
                       ?? string.Empty,

            Volume = FormatLong(quote.RegularMarketVolume),

            AvgVolume = FormatLong(
                quote.AverageDailyVolume3Month
                ?? quote.AverageDailyVolume10Day),

            Open = FormatDecimal(quote.RegularMarketOpen),
            PreviousClose = FormatDecimal(quote.RegularMarketPreviousClose),

            Eps = FormatDecimal(quote.EpsTrailingTwelveMonths),
            Pe = FormatNullableDecimal(quote.TrailingPE ?? quote.ForwardPE),

            EarningsAnnouncement = FormatUnixDate(quote.EarningsTimestamp),
            Timestamp = FormatUnixDateTime(quote.RegularMarketTime)
        };
    }

    private static string FormatDecimal(decimal? value)
    {
        return value is null
            ? string.Empty
            : value.Value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string? FormatNullableDecimal(decimal? value)
    {
        return value is null
            ? null
            : value.Value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatPercentage(decimal? value)
    {
        return value is null
            ? string.Empty
            : value.Value.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    private static string FormatLong(long? value)
    {
        return value is null
            ? string.Empty
            : value.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatUnixDate(long? unixSeconds)
    {
        return unixSeconds is null
            ? string.Empty
            : DateTimeOffset
                .FromUnixTimeSeconds(unixSeconds.Value)
                .UtcDateTime
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static string FormatUnixDateTime(long? unixSeconds)
    {
        return unixSeconds is null
            ? string.Empty
            : DateTimeOffset
                .FromUnixTimeSeconds(unixSeconds.Value)
                .UtcDateTime
                .ToString("O", CultureInfo.InvariantCulture);
    }
}