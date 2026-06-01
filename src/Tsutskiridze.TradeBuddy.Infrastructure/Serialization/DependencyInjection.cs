using Microsoft.Extensions.DependencyInjection;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Serialization;

public static class DependencyInjection
{
    public static IServiceCollection AddJsonSerializationOptions(this IServiceCollection services)
    {
        services.AddSingleton<InfraJsonSerializerOptions>();
        return services;
    }
}
