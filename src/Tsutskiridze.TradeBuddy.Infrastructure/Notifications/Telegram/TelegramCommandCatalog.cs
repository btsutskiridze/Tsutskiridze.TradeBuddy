namespace Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

public sealed record TelegramCommandDefinition(
    string Command,
    string Description);

public static class TelegramCommandCatalog
{
    public static readonly TelegramCommandDefinition Help =
        new("/help", "List all available commands");

    public static readonly TelegramCommandDefinition Activate =
        new("/start", "Activate the bot. e.g: /start de1few34f67fef8fe0");

    public static readonly TelegramCommandDefinition Alert =
        new("/set", "Create a stock alert. e.g: /set NVDA above 300");

    public static readonly TelegramCommandDefinition MyAlerts =
        new("/list", "List all your active alerts");

    public static readonly TelegramCommandDefinition StockQuote =
        new("/price", "Get current price and analysis. e.g: /price NVDA");

    public static readonly TelegramCommandDefinition RemoveAlert =
        new("/del", "Delete a price alert. e.g: /del NVDA above 300");

    public static readonly IReadOnlyList<TelegramCommandDefinition> All =
    [
        Help,
        Activate,
        Alert,
        MyAlerts,
        StockQuote,
        RemoveAlert
    ];
}