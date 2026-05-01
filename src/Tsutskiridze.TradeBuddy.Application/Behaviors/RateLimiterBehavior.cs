using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.RateLimiting;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;

namespace Tsutskiridze.TradeBuddy.Application.Behaviors;

public class RateLimiterBehavior<TMessage, TResponse> : MessagePreProcessor<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly IRateLimiter _rateLimiter;

    public RateLimiterBehavior(IRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }

    protected override async ValueTask Handle(TMessage message, CancellationToken ct)
    {
        if (message is not IRateLimitedMessage limitedMessage)
        {
            return;
        }
        
        var result = await _rateLimiter.CheckAsync(
            limitedMessage.Id,
            limitedMessage.Command,
            limitedMessage.Policy,
            ct
        );

        if (!result.IsAllowed)
        {
            throw new ApplicationLayerException("Try after " + result.RetryAfter!.Value.Seconds + " seconds", 429);
        }
    }
}