using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public static class DependencyInjection
{
    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxProcessorOptions>(configuration.GetSection(OutboxProcessorOptions.SectionName));

        services.AddHostedService<DailyStrategyMonitorJob>();
        // services.AddHostedService<TestNasdaqStrategyReportJob>();
        services.AddHostedService<OutboxProcessorJob>();

        return services;
    }
}