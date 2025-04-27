
using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

public static class DbConfigurationExtension
{

    public static IServiceCollection AddDbConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                pg =>
                {
                    pg.EnableRetryOnFailure();
                    pg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
        );

        return services;
    }


}