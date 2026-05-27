namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;

internal sealed class YahooCrumbCache:IDisposable
{
    //todo: check the approach without semaphore slim
    private readonly SemaphoreSlim _lock = new(1, 1);
    public YahooCrumbSession? Session { get; set; }
    public Task WaitAsync(CancellationToken ct) => _lock.WaitAsync(ct);
    public void Release() => _lock.Release();
    internal sealed record YahooCrumbSession(string Crumb, DateTimeOffset ExpiresAtUtc);

    public void Dispose()
    {
        _lock.Dispose();
    }

    ~YahooCrumbCache()
    {
        
    }
}