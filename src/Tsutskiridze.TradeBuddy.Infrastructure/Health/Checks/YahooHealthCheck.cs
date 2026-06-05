using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Health.Checks;

internal sealed class YahooHealthCheck(
    IHttpClientFactory httpClientFactory,
    IOptions<YahooOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var yahooOptions = options.Value;

        if (string.IsNullOrWhiteSpace(yahooOptions.Query2ApiUrl))
        {
            return HealthCheckResult.Degraded("Yahoo Query2 API URL is not configured.");
        }

        if (!Uri.TryCreate(yahooOptions.Query2ApiUrl, UriKind.Absolute, out var baseUri))
        {
            return HealthCheckResult.Degraded("Yahoo Query2 API URL is invalid.");
        }

        try
        {
            var client = httpClientFactory.CreateClient(HealthCheckHttpClientNames.Yahoo);

            using var request = new HttpRequestMessage(HttpMethod.Head, baseUri);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Yahoo Query2 API is reachable.");
            }

            return HealthCheckResult.Degraded(
                $"Yahoo Query2 API returned {(int)response.StatusCode} {response.ReasonPhrase}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Degraded("Yahoo health check timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Yahoo Query2 API is unreachable.", ex);
        }
    }
}
