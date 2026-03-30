namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public sealed record TelegramCommandDefinition(
    string Command,
    string Description);

public static class TelegramCommandCatalog
{
    public static readonly TelegramCommandDefinition Help =
        new("/help", "List all available commands");

    public static readonly TelegramCommandDefinition Activate =
        new("/activate", "Activate the bot. e.g: /activate de1few34f67fef8fe0");

    public static readonly TelegramCommandDefinition Alert =
        new("/alert", "Create a stock alert. e.g: /alert NVDA above 300");

    public static readonly TelegramCommandDefinition MyAlerts = 
        new("/myAlerts", "List all your alerts e.g: /myAlerts");

    public static readonly TelegramCommandDefinition StockQuote = 
        new("/quote", "Get stock analysis for a symbol. e.g: /quote NVDA");
    
    public static readonly IReadOnlyList<TelegramCommandDefinition> All =
    [
        Help,
        Activate,
        Alert,
        MyAlerts,
        StockQuote
    ];
}