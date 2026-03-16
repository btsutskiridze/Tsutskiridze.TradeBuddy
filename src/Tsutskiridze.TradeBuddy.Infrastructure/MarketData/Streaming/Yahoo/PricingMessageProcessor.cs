using MarketData;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Streaming;
using Tsutskiridze.TradeBuddy.Application.IntegrationEvents.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Streaming.Yahoo
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
                    new PricingUpdatedIntegrationEvent(update.Id, (decimal)update.Price), ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to parse pricing message");
            }
        }
    }
}