using Tsutskiridze.TradeBuddy.Application.Options;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.Gemini;
using Tsutskiridze.TradeBuddy.Infrastructure.AI.OpenAI;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.AlphaVantage;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.FinancialModelingPrep;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo;
using Tsutskiridze.TradeBuddy.Infrastructure.Mails;
using Tsutskiridze.TradeBuddy.Infrastructure.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Extensions
{
    public static class OptionsConfigurationExtensions
    {
        public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
            services.Configure<AlphaVantageOptions>(configuration.GetSection(AlphaVantageOptions.SectionName));
            services.Configure<FinancialModelingPrepOptions>(configuration.GetSection(FinancialModelingPrepOptions.SectionName));
            services.Configure<RedditOptions>(configuration.GetSection(RedditOptions.SectionName));
            services.Configure<FinnhubOptions>(configuration.GetSection(FinnhubOptions.SectionName));
            services.Configure<YahooOptions>(configuration.GetSection(YahooOptions.SectionName));
            services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
            services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
            services.Configure<TelegramClientOptions>(configuration.GetSection(TelegramClientOptions.SectionName));
            services.Configure<TelegramBotOptions>(configuration.GetSection(TelegramBotOptions.SectionName));

            return services;
        }
    }
}
