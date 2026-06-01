namespace Tsutskiridze.TradeBuddy.API.Telegram;

public sealed record TelegramCommandDefinition(
    string Command,
    string Description,
    string Usage);

public static class TelegramCommandCatalog
{
    public static readonly TelegramCommandDefinition Help =
        new(
            "/help",
            "List all available commands",
            "/help");

    public static readonly TelegramCommandDefinition Activate =
        new(
            "/start",
            "Activate the bot",
            "/start <activation-token>\nExample: /start de1few34f67fef8fe0");

    public static readonly TelegramCommandDefinition Alert =
        new(
            "/set",
            "Create a stock alert",
            "/set <symbol> <above|below> <price>\nExample: /set NVDA above 300");

    public static readonly TelegramCommandDefinition MyAlerts =
        new(
            "/list",
            "List all your active alerts",
            "/list");

    public static readonly TelegramCommandDefinition AnalyseStock =
        new(
            "/price",
            "Get current price and analysis",
            "/price <symbol>\nExample: /price NVDA");

    public static readonly TelegramCommandDefinition RemoveAlert =
        new(
            "/del",
            "Delete a price alert",
            "/del <symbol> <above|below> <price>\nExample: /del NVDA above 300");
    
    public static readonly TelegramCommandDefinition AddStrategy =
        new(
            "/addstrategy",
            "Create a daily EMA/ADX/ATR strategy",
            "/addstrategy ema <fast-period> <slow-period> adx <period> <threshold> <lookback> atr <period> <initial-stop-multiplier> <trailing-stop-multiplier> <trailing-activation-multiplier>\nExample: /addstrategy ema 10 20 adx 14 25 3 atr 14 2 3 1");

    public static readonly TelegramCommandDefinition ListStrategies =
        new(
            "/mystrategies",
            "List your active trade strategies",
            "/mystrategies [ts-id]\nExample: /mystrategies ts_2\nExample: /mystrategies"
        );

    public static readonly TelegramCommandDefinition DeleteStrategy =
        new(
            "/delstrategy",
            "Delete a trade strategy",
            "/delstrategy <ts-id>\nExample: /delstrategy ts_3"
        );

    public static readonly TelegramCommandDefinition MonitorStrategy =
        new(
            "/monitor",
            "Activate a strategy monitor",
            "/monitor <symbol> <ts-id>\nExample: /monitor NVDA ts_3"
        );
    
    
    public static readonly IReadOnlyList<TelegramCommandDefinition> All =
    [
        Help,
        Activate,
        Alert,
        MyAlerts,
        AnalyseStock,
        AddStrategy,
        ListStrategies,
        RemoveAlert,
        DeleteStrategy,
        MonitorStrategy
    ];
}
