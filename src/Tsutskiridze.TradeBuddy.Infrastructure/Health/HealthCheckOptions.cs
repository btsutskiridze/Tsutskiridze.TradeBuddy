namespace Tsutskiridze.TradeBuddy.Infrastructure.Health;

public sealed class HealthCheckOptions
{
    public const string SectionName = "HealthChecks";

    public int PostgresTimeoutSeconds { get; set; } = 3;

    public int IntegrationTimeoutSeconds { get; set; } = 5;
}
