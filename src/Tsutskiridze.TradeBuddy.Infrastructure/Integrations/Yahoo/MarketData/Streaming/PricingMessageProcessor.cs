using MarketData;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Events;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming.Abstractions;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Streaming
{
    public class PricingMessageProcessor : IPricingMessageProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PricingMessageProcessor> _log;


        public PricingMessageProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<PricingMessageProcessor> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        public async Task ProcessAsync(string json, CancellationToken ct)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var data = JObject.Parse(json);
                var base64Message = data["message"]?.ToString();
                if (string.IsNullOrEmpty(base64Message)) return;

                var update = PricingData.Parser.ParseFrom(Convert.FromBase64String(base64Message));
                _log.LogDebug("Parsed update {Symbol} @ {Price}", update.Id, update.Price);

                await mediator.Publish(
                    new MarketPriceUpdatedApplicationEvent(update.Id, (decimal)update.Price), ct);
            }
            catch (ConcurrencyConflictException ex)
            {
                _log.LogWarning(ex, "Concurrency conflict");
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to parse pricing message");
            }
        }
    }
}
