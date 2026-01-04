using Microsoft.EntityFrameworkCore;
using Tsutskiridze.TradeBuddy.Application.Interfaces.Database;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Context;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class DatabaseServiceExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseNpgsql(
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

            services.AddScoped<IAppDbContext, AppDbContext>();

            return services;
        }
    }
}