using Microsoft.Extensions.Logging;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;

public sealed class NasdaqSymbolDirectoryProvider : INasdaqSymbolDirectoryProvider
{
    private static readonly Uri NasdaqListedUri = new("dynamic/SymDir/nasdaqlisted.txt", UriKind.Relative);
    private static readonly Uri OtherListedUri = new("dynamic/SymDir/otherlisted.txt", UriKind.Relative);
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);
    private static readonly string CacheDirectory = Path.Combine(
        AppContext.BaseDirectory,
        "cache",
        "nasdaq");
    private static readonly string NasdaqListedCachePath = Path.Combine(CacheDirectory, "nasdaqlisted.txt");
    private static readonly string OtherListedCachePath = Path.Combine(CacheDirectory, "otherlisted.txt");

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NasdaqSymbolDirectoryProvider> _logger;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public NasdaqSymbolDirectoryProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<NasdaqSymbolDirectoryProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<NasdaqSymbolLookupResult> StockSymbolExists(
        string symbol,
        CancellationToken ct = default)
    {
        try
        {
            var symbols = await GetSymbols(ct);
            return NasdaqSymbolLookupResult.Success(symbols.Contains(symbol));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load Nasdaq symbol directory");
            return NasdaqSymbolLookupResult.Failure();
        }
    }

    public async Task<IReadOnlyList<NasdaqListedStock>> GetNasdaqListedStocks(
        CancellationToken ct = default)
    {
        await _cacheLock.WaitAsync(ct);
        try
        {
            var content = await GetDirectoryContent(NasdaqListedUri, NasdaqListedCachePath, ct);

            return ParseNasdaqListedStocks(content)
                .OrderBy(x => x.Symbol, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<HashSet<string>> GetSymbols(CancellationToken ct)
    {
        await _cacheLock.WaitAsync(ct);
        try
        {
            var nasdaqListedTask = GetDirectoryContent(NasdaqListedUri, NasdaqListedCachePath, ct);
            var otherListedTask = GetDirectoryContent(OtherListedUri, OtherListedCachePath, ct);

            await Task.WhenAll(nasdaqListedTask, otherListedTask);

            var symbols = ParseNasdaqListedSymbols(nasdaqListedTask.Result);
            symbols.UnionWith(ParseOtherListedSymbols(otherListedTask.Result));

            if (symbols.Count == 0)
                throw new InvalidOperationException("Nasdaq symbol directory contained no symbols.");

            return symbols;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<string> GetDirectoryContent(
        Uri uri,
        string cachePath,
        CancellationToken ct)
    {
        if (IsCacheFresh(cachePath))
            return await File.ReadAllTextAsync(cachePath, ct);

        var content = await FetchDirectoryContent(uri, ct);

        Directory.CreateDirectory(CacheDirectory);
        await File.WriteAllTextAsync(cachePath, content, ct);

        return content;
    }

    private async Task<string> FetchDirectoryContent(
        Uri uri,
        CancellationToken ct)
    {
        var httpClient = _httpClientFactory.CreateClient(NasdaqOptions.SymbolDirectoryClientName);

        using var response = await httpClient.GetAsync(uri, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }

    private static bool IsCacheFresh(string cachePath)
    {
        if (!File.Exists(cachePath))
            return false;

        var cacheCreatedAt = new DateTimeOffset(File.GetLastWriteTimeUtc(cachePath), TimeSpan.Zero);
        return cacheCreatedAt.Add(CacheDuration) > DateTimeOffset.UtcNow;
    }

    private static HashSet<string> ParseNasdaqListedSymbols(string content)
    {
        var symbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in GetDataLines(content))
        {
            var columns = line.Split('|');
            if (columns.Length < 4 || IsTestIssue(columns[3]))
                continue;

            AddSymbol(symbols, columns[0]);
        }

        return symbols;
    }

    private static IReadOnlyList<NasdaqListedStock> ParseNasdaqListedStocks(string content)
    {
        var stocks = new List<NasdaqListedStock>();

        foreach (var line in GetDataLines(content))
        {
            var columns = line.Split('|');
            if (columns.Length < 7 || IsTestIssue(columns[3]) || IsEtf(columns[6]))
                continue;

            var symbol = NormalizeSymbol(columns[0]);
            var securityName = columns[1].Trim();

            if (string.IsNullOrWhiteSpace(symbol))
                continue;

            stocks.Add(new NasdaqListedStock(symbol, securityName));
        }

        return stocks;
    }

    private static HashSet<string> ParseOtherListedSymbols(string content)
    {
        var symbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in GetDataLines(content))
        {
            var columns = line.Split('|');
            if (columns.Length < 7 || IsTestIssue(columns[6]))
                continue;

            AddSymbol(symbols, columns[0]);

            if (columns.Length > 7)
                AddSymbol(symbols, columns[7]);
        }

        return symbols;
    }

    private static IEnumerable<string> GetDataLines(string content)
    {
        return content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line =>
                !line.StartsWith("Symbol|", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith("ACT Symbol|", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith("File Creation Time:", StringComparison.OrdinalIgnoreCase));
    }

    private static void AddSymbol(HashSet<string> symbols, string value)
    {
        var symbol = NormalizeSymbol(value);
        if (!string.IsNullOrWhiteSpace(symbol))
            symbols.Add(symbol);
    }

    private static string NormalizeSymbol(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static bool IsTestIssue(string value)
    {
        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEtf(string value)
    {
        return string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
    }
}
