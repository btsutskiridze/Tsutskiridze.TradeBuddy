using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram.Config;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Health.Checks;

internal sealed class TelegramHealthCheck(
    ITelegramBotClient telegramBotClient,
    IOptions<TelegramClientOptions> options) : IHealthCheck
{
    private static readonly string[] PlaceholderTokenPrefixes = ["your-", "placeholder"];

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var telegramOptions = options.Value;

        if (string.IsNullOrWhiteSpace(telegramOptions.BotToken)
            || PlaceholderTokenPrefixes.Any(prefix =>
                telegramOptions.BotToken.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return HealthCheckResult.Degraded("Telegram bot token is not configured.");
        }

        try
        {
            var botUser = await telegramBotClient.GetMe(cancellationToken);

            return HealthCheckResult.Healthy($"Telegram bot @{botUser.Username} is reachable.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Degraded("Telegram health check timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Telegram API is unreachable.", ex);
        }
    }
}
