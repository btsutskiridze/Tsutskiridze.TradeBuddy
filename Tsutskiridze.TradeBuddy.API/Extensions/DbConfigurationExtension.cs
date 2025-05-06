
using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

public static class DbConfigurationExtension
{

    public static IServiceCollection AddDbConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt
            .UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                pg =>
                {
                    pg.EnableRetryOnFailure();
                    pg.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    pg.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                }
            )
            .UseSnakeCaseNamingConvention()
        );

        services.AddTransient<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }


}