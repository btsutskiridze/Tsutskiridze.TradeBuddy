using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming;
using Tsutskiridze.TradeBuddy.Core.Aggregates.Stocks.Event;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo
{
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly IMarketDataTransportClient _transport;
        private readonly ILogger<SubscriptionManager> _log;
        private ImmutableHashSet<string> _watched = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _lock = new(1, 1);

        public SubscriptionManager(
            IServiceScopeFactory scopes,
            IMarketDataTransportClient transport,
            ILogger<SubscriptionManager> log)
        {
            _scopes = scopes;
            _transport = transport;
            _log = log;
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stocks = await db.Stocks
                           .AsNoTracking()
                           .Where(s => s.IsWatched)
                           .Select(s => s.Symbol)
                           .ToListAsync(ct);

            _watched = stocks.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            if (_watched.Count == 0) return;

            await _transport.SendAsync(JsonConvert.SerializeObject(new { subscribe = _watched }), ct);
        }

        public async ValueTask Handle(StockWatchStatusChangedDomainEvent evt, CancellationToken ct)
        {
            var shouldSub = evt.IsWatched && !_watched.Contains(evt.Symbol);
            var shouldUnsub = !evt.IsWatched && _watched.Contains(evt.Symbol);

            if (!shouldSub && !shouldUnsub) return;

            await _lock.WaitAsync(ct);
            try
            {
                _watched = shouldSub
                    ? _watched.Add(evt.Symbol)
                    : _watched.Remove(evt.Symbol);

                _log.LogInformation("All watched stocks: {Watched}", _watched);

                var payload = new Dictionary<string, IEnumerable<string>>();
                if (shouldSub) payload["subscribe"] = [evt.Symbol];
                if (shouldUnsub) payload["unsubscribe"] = [evt.Symbol];

                var msg = JsonConvert.SerializeObject(payload);
                await _transport.SendAsync(msg, ct);
                _log.LogInformation("Subscriptions updated: {Msg}", msg);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
