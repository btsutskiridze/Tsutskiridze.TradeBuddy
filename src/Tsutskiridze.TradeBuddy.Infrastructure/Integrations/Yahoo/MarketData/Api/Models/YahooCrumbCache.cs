using Microsoft.Extensions.Options;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Api.Models;

internal sealed class YahooCrumbCache
{
    //todo: check the approach without semaphore slim
    private YahooCrumbSession? _session;

    private Task<YahooCrumbSession>? _cacheRefreshTask;

    public async Task<YahooCrumbSession> GetOrRefreshAsync(
        bool forceRefresh,
        Func<CancellationToken, Task<YahooCrumbSession>> refreshSessionAsync,
        CancellationToken ct
    )
    {
        var current = Volatile.Read(ref _session);

        if (!forceRefresh && IsValid(current))
        {
            return current!;
        }

        var refreshTask = Volatile.Read(ref _cacheRefreshTask);

        if (refreshTask is not null) return await refreshTask.WaitAsync(ct);
        
        var tcs = new TaskCompletionSource<YahooCrumbSession>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
            
        var existingTask = Interlocked.CompareExchange(
            ref _cacheRefreshTask,
            tcs.Task,
            null
        );

        if (existingTask is not null)
        {
            return await existingTask.WaitAsync(ct);
        }
        
        _ = RefreshAndCompleteAsync(
            tcs,
            refreshSessionAsync
        );
        
        return await tcs.Task.WaitAsync(ct);
    }

    private async Task RefreshAndCompleteAsync(
        TaskCompletionSource<YahooCrumbSession> tcs,
        Func<CancellationToken, Task<YahooCrumbSession>> refreshSessionAsync)
    {
        try
        {
            var newSession = await refreshSessionAsync(CancellationToken.None);

            Volatile.Write(ref _session, newSession);

            tcs.SetResult(newSession);
        }
        catch (OperationCanceledException ex)
        {
            tcs.TrySetCanceled(ex.CancellationToken);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
        finally
        {
            await Interlocked.CompareExchange(
                ref _cacheRefreshTask,
                null,
                tcs.Task)!;
        }
    }

    private bool IsValid(YahooCrumbSession? current)
    {
        return current is not null && current.ExpiresAtUtc > DateTimeOffset.UtcNow;
    }

    public void Invalidate()
    {
        // UPDATED:
        // Replaces direct public mutation: _crumbCache.Session = null.
        Volatile.Write(ref _session, null);
    }

    internal sealed record YahooCrumbSession(string Crumb, DateTimeOffset ExpiresAtUtc);
}