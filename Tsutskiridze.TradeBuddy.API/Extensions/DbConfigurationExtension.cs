using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Tsutskiridze.TradeBuddy.Core.Repositories;
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

            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            services.AddScoped<IChatRepository, ChatRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IPriceAlertReadRepository, PriceAlertReadRepository>();

            return services;
        }
    }
}