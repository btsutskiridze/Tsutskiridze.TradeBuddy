using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Data;
using SharedKernel.Events.DomainEventsDispatching;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.DomainEventsDispatching;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Repositories;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                    configuration.GetConnectionString("Postgres"),
                    npgsql =>
                    {
                        
                        // npgsql.EnableRetryOnFailure();  //todo: fix this user transaction not working situation and enable retry
                        npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                    })
                .UseSnakeCaseNamingConvention());

        services.AddScoped(typeof(IReadRepository<>), typeof(EfReadRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        
        services.AddScoped<IDomainEventAccessor, DomainEventAccessor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        
        services.AddSingleton<IDbExceptionClassifier, PostgresDbExceptionClassifier>();
        
        return services;
    }
    
    public static async Task ApplyDatabaseMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
    }
}
