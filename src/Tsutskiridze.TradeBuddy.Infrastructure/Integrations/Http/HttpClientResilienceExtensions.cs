using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Http;

internal static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder AddResiliencePipeline(
        this IHttpClientBuilder builder,
        string pipelineName)
    {
        builder.ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan);

        builder.AddResilienceHandler($"{pipelineName}-resilience", static pipeline =>
        {
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                MaxDelay = TimeSpan.FromSeconds(10),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(15));
        });

        return builder;
    }
}
