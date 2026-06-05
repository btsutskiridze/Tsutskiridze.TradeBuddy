using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Health.Checks;

internal sealed class PostgresHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("PostgreSQL is reachable.");
            }

            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL health check timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is unreachable.", ex);
        }
    }
}
