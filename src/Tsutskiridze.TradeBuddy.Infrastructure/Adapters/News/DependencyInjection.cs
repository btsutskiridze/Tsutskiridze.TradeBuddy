using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Adapters.News;

public static class DependencyInjection
{
    public static IServiceCollection AddNewsServices(this IServiceCollection services)
    {
        services.AddScoped<IStockNewsReader, StockNewsReader>();

        return services;
    }
}
