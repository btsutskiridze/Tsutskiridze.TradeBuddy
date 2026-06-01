using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;
using Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming
{
    public sealed class SubscriptionManager : ISubscriptionManager
    {
        private readonly ConcurrentDictionary<string, byte> _watched = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

        private readonly IServiceScopeFactory _scopes;
        private readonly IMarketDataTransportClient _transport;
        private readonly ILogger<SubscriptionManager> _log;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SubscriptionManager(
            IServiceScopeFactory scopes,
            IMarketDataTransportClient transport,
            ILogger<SubscriptionManager> log,
            InfraJsonSerializerOptions jsonSerializerOptions)
        {
            _scopes = scopes;
            _transport = transport;
            _log = log;
            _jsonSerializerOptions = jsonSerializerOptions.Options;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var symbols = await db.Stocks
                .AsNoTracking()
                .Where(s => s.IsWatched)
                .Select(s => s.Symbol)
                .ToListAsync(ct);

            foreach (var stock in symbols)
            {
                _watched.TryAdd(stock, 0);
            }

            if (_watched.IsEmpty) return;

            var msg = JsonSerializer.Serialize(new { subscribe = symbols }, _jsonSerializerOptions);

            await _transport.SendAsync(msg, ct);

            _log.LogInformation("Initial Yahoo subscriptions sent: {Msg}", msg);
        }

        public async Task SubscribeStockPriceAsync(string symbol, CancellationToken ct)
        {
            var symbolLock = _locks.GetOrAdd(symbol, _ => new SemaphoreSlim(1, 1));
            await symbolLock.WaitAsync(ct);

            try
            {
                if (!_watched.TryAdd(symbol, 0))
                    return;

                var msg = JsonSerializer.Serialize(new
                {
                    subscribe = new[] { symbol }
                }, _jsonSerializerOptions);

                await _transport.SendAsync(msg, ct);

                _log.LogInformation("All watched stocks: {Watched}", string.Join(", ", _watched.Keys));
                _log.LogInformation("Subscriptions updated: {Msg}", msg);
            }
            catch
            {
                _watched.TryRemove(symbol, out _);
                throw;
            }
            finally
            {
                symbolLock.Release();
            }
        }

        public async Task UnsubscribeStockPriceAsync(string symbol, CancellationToken ct)
        {
            var symbolLock = _locks.GetOrAdd(symbol, _ => new SemaphoreSlim(1, 1));
            await symbolLock.WaitAsync(ct);

            try
            {
                if (!_watched.TryRemove(symbol, out _))
                    return;

                var msg = JsonSerializer.Serialize(new
                {
                    unsubscribe = new[] { symbol }
                }, _jsonSerializerOptions);

                await _transport.SendAsync(msg, ct);

                _log.LogInformation("All watched stocks: {Watched}", string.Join(", ", _watched.Keys));
                _log.LogInformation("Subscriptions updated: {Msg}", msg);
            }
            catch
            {
                _watched.TryAdd(symbol, 0);
                throw;
            }
            finally
            {
                symbolLock.Release();
            }
        }
    }
}