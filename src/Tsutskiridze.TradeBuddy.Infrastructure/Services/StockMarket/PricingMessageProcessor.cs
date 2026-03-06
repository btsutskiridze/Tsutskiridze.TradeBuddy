using MarketData;
using Mediator;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tsutskiridze.TradeBuddy.Application.Contracts.Services.StockMarket;
using Tsutskiridze.TradeBuddy.Application.Events;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Services.StockMarket
{
    public class PricingMessageProcessor : IPricingMessageProcessor
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PricingMessageProcessor> _log;

        public PricingMessageProcessor(
            IMediator mediator,
            ILogger<PricingMessageProcessor> log)
        {
            _mediator = mediator;
            _log = log;
        }

        public async Task ProcessAsync(string json, CancellationToken ct)
        {
            try
            {
                var obj = JObject.Parse(json);
                var b64 = obj["message"]?.ToString();
                if (string.IsNullOrEmpty(b64)) return;

                var update = PricingData.Parser.ParseFrom(Convert.FromBase64String(b64));
                _log.LogDebug("Parsed update {Symbol} @ {Price}", update.Id, update.Price);

                await _mediator.Publish(
                  new PricingUpdated(update.Id, (decimal)update.Price), ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to parse pricing message");
            }
        }
    }
}
