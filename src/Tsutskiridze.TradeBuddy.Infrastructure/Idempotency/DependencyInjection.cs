using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Idempotency;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Idempotency;

public static class DependencyInjection
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIdempotency, IdempotencyService>();

        return services;
    }
}