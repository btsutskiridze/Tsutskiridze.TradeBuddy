using Microsoft.Extensions.DependencyInjection;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Services;

namespace Tsutskiridze.TradeBuddy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddTransient<StockAnalysisPromptBuilder>();
        services.AddTransient<StockAnalysisGenerator>();
        
        services.AddScoped<IActiveChatProvider, ActiveChatProvider>();
        
        return services;
    }
}
