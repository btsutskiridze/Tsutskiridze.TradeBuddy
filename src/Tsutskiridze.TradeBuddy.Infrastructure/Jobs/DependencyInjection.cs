using Microsoft.Extensions.DependencyInjection;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public static class DependencyInjection
{

    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddHostedService<DailyStrategyMonitorJob>();

        return services;
    }
}