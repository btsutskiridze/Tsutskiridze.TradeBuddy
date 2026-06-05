using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.Health.Checks;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Health;

public static class DependencyInjection
{
    public static IServiceCollection AddTradeBuddyHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<HealthCheckOptions>(
            configuration.GetSection(HealthCheckOptions.SectionName));

        services.AddHttpClient(HealthCheckHttpClientNames.Yahoo, (serviceProvider, client) =>
        {
            var yahooOptions = serviceProvider.GetRequiredService<IOptions<YahooOptions>>().Value;
            var healthOptions = serviceProvider.GetRequiredService<IOptions<HealthCheckOptions>>().Value;

            if (Uri.TryCreate(yahooOptions.Query2ApiUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }

            client.Timeout = TimeSpan.FromSeconds(healthOptions.IntegrationTimeoutSeconds);
        });

        var healthOptions = configuration
            .GetSection(HealthCheckOptions.SectionName)
            .Get<HealthCheckOptions>() ?? new HealthCheckOptions();

        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy("Application process is running."),
                tags: [HealthCheckTags.Live])
            .AddCheck<PostgresHealthCheck>(
                "postgres",
                failureStatus: HealthStatus.Unhealthy,
                tags: [HealthCheckTags.Ready],
                timeout: TimeSpan.FromSeconds(healthOptions.PostgresTimeoutSeconds))
            .AddCheck<YahooHealthCheck>(
                "yahoo",
                failureStatus: HealthStatus.Degraded,
                tags: [HealthCheckTags.Ready],
                timeout: TimeSpan.FromSeconds(healthOptions.IntegrationTimeoutSeconds))
            .AddCheck<TelegramHealthCheck>(
                "telegram",
                failureStatus: HealthStatus.Degraded,
                tags: [HealthCheckTags.Ready],
                timeout: TimeSpan.FromSeconds(healthOptions.IntegrationTimeoutSeconds));

        return services;
    }
}
