using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

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

            services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));
            services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            return services;
        }
    }
}