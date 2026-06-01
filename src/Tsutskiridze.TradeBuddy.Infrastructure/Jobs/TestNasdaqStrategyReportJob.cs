using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.ValueObjects;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Nasdaq.SymbolDirectory;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Telegram;
using Tsutskiridze.TradeBuddy.Infrastructure.TextFormatting.Telegram;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public sealed class TestNasdaqStrategyReportJob : BackgroundService
{
    private const int FastEmaPeriod = 10;
    private const int SlowEmaPeriod = 20;
    private const int AdxPeriod = 14;
    private const decimal AdxThreshold = 23m;
    private const int AdxNonFallingLookBackBars = 3;
    private const int AtrPeriod = 14;
    private const decimal AtrInitialStopMultiplier = 3m;
    private const decimal AtrTrailingStopMultiplier = 5m;
    private const int AtrTrailingActivationMultiplier = 1;
    private const int EvaluationYears = 2;
    private const int WarmupDays = 90;
    private const long PrivateChatId = 6039711590;
    private const int MaxConcurrencyOfNetworkRequests = 30; // <-- adjust as appropriate

    private static readonly TimeOnly RunAtEasternTime = new(17, 15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TestNasdaqStrategyReportJob> _logger;

    public TestNasdaqStrategyReportJob(
        IServiceScopeFactory scopeFactory,
        ILogger<TestNasdaqStrategyReportJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var easternTimeZone = GetEasternTimeZone();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunReport(ct);

                var nowUtc = DateTimeOffset.UtcNow;
                var nextRunUtc = GetNextWeekdayRunUtc(nowUtc, easternTimeZone);
                var delay = nextRunUtc - nowUtc;

                _logger.LogInformation(
                    "NASDAQ strategy report job next run: {NextRunUtc} UTC",
                    nextRunUtc);

                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NASDAQ strategy report job failed");

                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }
        }
    }

    private async Task RunReport(CancellationToken ct)
    {
        _logger.LogInformation("NASDAQ strategy report job started");

        await using var scope = _scopeFactory.CreateAsyncScope();

        var symbolDirectory = scope.ServiceProvider.GetRequiredService<INasdaqSymbolDirectoryProvider>();
        var market = scope.ServiceProvider.GetRequiredService<IMarketDataProvider>();
        var candleBuilder = scope.ServiceProvider.GetRequiredService<IEmaAdxAtrEvaluationCandleBuilder>();
        var telegramSender = scope.ServiceProvider.GetRequiredService<ITelegramSender>();

        var stocks = await symbolDirectory.GetNasdaqListedStocks(ct);
        var strategy = CreateStrategy();
        var rows = new ConcurrentBag<NasdaqStrategyReportRow>();

        _logger.LogInformation(
            "NASDAQ strategy report will evaluate {StockCount} stocks",
            stocks.Count);

        using var semaphore = new SemaphoreSlim(MaxConcurrencyOfNetworkRequests);

        var tasks = stocks.Select(async (stock, i) =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                ct.ThrowIfCancellationRequested();
                // Log progress for each stock before evaluation
                _logger.LogInformation("Evaluating stock {Current}/{Total}: {Symbol} ({SecurityName})",
                    i + 1, stocks.Count, stock.Symbol, stock.SecurityName);

                var row = await EvaluateStock(
                    stock,
                    strategy,
                    market,
                    candleBuilder,
                    ct);

                rows.Add(row);
                LogStockResult(row, i + 1, stocks.Count);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        var reportDirectory = GetReportDirectory();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var rowsList = rows.ToList();

        var reportPath = await WriteCsv(
            rowsList,
            reportDirectory,
            $"nasdaq-strategy-evaluation-{timestamp}.csv",
            ct);

        var profitableRows = rowsList
            .Where(IsProfitableStrategyResult)
            .OrderByDescending(x => x.TotalStrategyProfitPercent)
            .ToArray();

        var todayOpenedtopRows = profitableRows
            .Where(x => 
                x.TotalStrategyProfitPercent > 40
                && x.WinningTradeCount > x.LosingTradeCount
                && x.CurrentEntryDate.HasValue && x.CurrentEntryDate.Value == DateOnly.FromDateTime(DateTime.UtcNow)
            )
            .ToArray();

        await SendTelegramNotificationOfTopRows(telegramSender, todayOpenedtopRows, ct);
   
        var profitableReportPath = await WriteCsv(
            profitableRows,
            reportDirectory,
            $"nasdaq-strategy-evaluation-profitable-{timestamp}.csv",
            ct);

        _logger.LogInformation(
            "NASDAQ strategy report job completed. Report path: {ReportPath}. Profitable report path: {ProfitableReportPath}. Profitable symbols: {ProfitableSymbolCount}",
            reportPath,
            profitableReportPath,
            profitableRows.Length);
    }

    private static async Task SendTelegramNotificationOfTopRows(
        ITelegramSender telegramSender,
        NasdaqStrategyReportRow[] todayOpenedTopRows,
        CancellationToken ct)
    {
        if (todayOpenedTopRows.Length == 0)
            return;

        foreach (var row in todayOpenedTopRows)
        {
            var message = BuildTelegramAlertMessage(row);

            await telegramSender.Send(
                new TelegramOutgoingMessage(PrivateChatId, message, ParseMode.Markdown),
                ct);
        }
    }

    private static string BuildTelegramAlertMessage(NasdaqStrategyReportRow row)
    {
        var position = string.IsNullOrWhiteSpace(row.CurrentSide)
            ? "None"
            : $"{row.CurrentSide}/{row.CurrentAction}";
        var entryDate = row.CurrentEntryDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "n/a";
        var currentPrice = row.CurrentPrice.HasValue
            ? TelegramValueFormatter.Decimal(row.CurrentPrice.Value)
            : "n/a";

        var sb = new StringBuilder();

        sb.AppendLine("📊 *NASDAQ Strategy Alert*");
        sb.AppendLine($"Symbol: *{row.Symbol}* | *{row.SecurityName}*");
        sb.AppendLine();
        sb.AppendLine($"💵 *Price:* {currentPrice}");
        sb.AppendLine($"📍 *Position:* {position}");
        sb.AppendLine($"📅 *Entry Date:* {entryDate}");
        sb.AppendLine();
        sb.AppendLine(
            $"📈 *Trades:* {row.TradeCount} | Wins: *{row.WinningTradeCount}* | Losses: *{row.LosingTradeCount}*");
        sb.AppendLine();
        sb.AppendLine($"🟢 *Total Strategy PnL:* {TelegramValueFormatter.Percent(row.TotalStrategyProfitPercent)}");
        sb.AppendLine($"📂 *Open Position PnL:* {TelegramValueFormatter.Percent(row.OpenStrategyProfitPercent)}");
        sb.AppendLine($"💰 *Realized PnL:* {TelegramValueFormatter.Percent(row.RealizedStrategyProfitPercent)}");
        sb.AppendLine($"📊 *Buy & Hold:* {TelegramValueFormatter.Percent(row.BuyAndHoldProfitPercent)}");

        if (!string.IsNullOrWhiteSpace(row.LatestReason))
        {
            sb.AppendLine();
            sb.AppendLine("📝 *Reason*");
            sb.AppendLine(row.LatestReason);
        }

        return sb.ToString();
    }

    private static async Task<NasdaqStrategyReportRow> EvaluateStock(
        NasdaqListedStock stock,
        TradeStrategy strategy,
        IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder candleBuilder,
        CancellationToken ct)
    {
        try
        {
            var historyDateRange = await market.GetClosedDailyDateRange(stock.Symbol, ct);
            var evaluationEnd = historyDateRange.To;
            var evaluationStart = evaluationEnd.AddYears(-EvaluationYears);
            var candleStart = evaluationStart.AddDays(-WarmupDays);
            var candles = await market.GetDailyCandles(
                stock.Symbol,
                candleStart,
                evaluationEnd,
                ct);

            var evaluationCandles = candles
                .Where(x => x.Date >= evaluationStart && x.Date <= evaluationEnd)
                .OrderBy(x => x.Date)
                .ToArray();

            if (evaluationCandles.Length == 0)
            {
                return NasdaqStrategyReportRow.Failed(
                    stock,
                    evaluationStart,
                    evaluationEnd,
                    "No candles returned for the evaluation window.");
            }

            var strategyCandles = candleBuilder.BuildDailyStrategyCandles(
                FastEmaPeriod,
                SlowEmaPeriod,
                AdxPeriod,
                AtrPeriod,
                candles);

            if (strategyCandles.Count == 0)
            {
                return NasdaqStrategyReportRow.Failed(
                    stock,
                    evaluationStart,
                    evaluationEnd,
                    "Not enough candles to calculate EMA, ADX, and ATR.");
            }

            var strategyEvaluationCandles = strategyCandles
                .Where(x => x.Date >= evaluationStart && x.Date <= evaluationEnd)
                .ToArray();

            if (strategyEvaluationCandles.Length == 0)
            {
                return NasdaqStrategyReportRow.Failed(
                    stock,
                    evaluationStart,
                    evaluationEnd,
                    "No strategy candles in the evaluation window.");
            }

            var monitor = CreateMonitor(stock.Symbol);
            var evaluationResults = EmaAdxAtrStrategyEvaluator.Replay(
                strategy,
                monitor,
                strategyEvaluationCandles);

            if (evaluationResults.Count == 0)
            {
                return NasdaqStrategyReportRow.Failed(
                    stock,
                    evaluationStart,
                    evaluationEnd,
                    "No strategy evaluation results in the evaluation window.");
            }

            return CreateReportRow(
                stock,
                evaluationStart,
                evaluationEnd,
                evaluationCandles,
                evaluationResults);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return NasdaqStrategyReportRow.Failed(
                stock,
                null,
                null,
                ex.Message);
        }
    }

    private static NasdaqStrategyReportRow CreateReportRow(
        NasdaqListedStock stock,
        DateOnly evaluationStart,
        DateOnly evaluationEnd,
        IReadOnlyList<MarketCandle> evaluationCandles,
        IReadOnlyList<StrategyEvaluationResult> evaluationResults)
    {
        var firstCandle = evaluationCandles[0];
        var lastCandle = evaluationCandles[^1];
        var latestResult = evaluationResults[^1];
        var closedTradeProfits = evaluationResults
            .Where(x => x.Action is StrategyAction.ExitLongByStop or StrategyAction.ExitLongByEmaCross)
            .Select(CalculateClosedTradeProfitPercent)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();

        var realizedProfitPercent = closedTradeProfits.Sum();
        var openProfitPercent = CalculateOpenProfitPercent(latestResult, lastCandle.Close);
        var buyAndHoldProfitPercent = CalculateProfitPercent(firstCandle.Close, lastCandle.Close);
        var currentPositionState = latestResult.PositionStateAfter;

        return new NasdaqStrategyReportRow(
            Symbol: stock.Symbol,
            SecurityName: stock.SecurityName,
            EvaluationStartDate: evaluationStart,
            EvaluationEndDate: evaluationEnd,
            FirstCandleDate: firstCandle.Date,
            LatestCandleDate: lastCandle.Date,
            OverallStartPrice: firstCandle.Close,
            CurrentPrice: lastCandle.Close,
            BuyAndHoldProfitPercent: buyAndHoldProfitPercent,
            RealizedStrategyProfitPercent: realizedProfitPercent,
            OpenStrategyProfitPercent: openProfitPercent,
            TotalStrategyProfitPercent: realizedProfitPercent + openProfitPercent,
            TradeCount: closedTradeProfits.Length,
            WinningTradeCount: closedTradeProfits.Count(x => x > 0m),
            LosingTradeCount: closedTradeProfits.Count(x => x <= 0m),
            CurrentAction: latestResult.Action.ToString(),
            CurrentSide: latestResult.PositionSideAfter.ToString(),
            CurrentEntryDate: currentPositionState.EntryDate is null
                ? null
                : DateOnly.FromDateTime(currentPositionState.EntryDate.Value),
            CurrentEntryPrice: currentPositionState.EntryPrice,
            CurrentActiveStop: latestResult.ActiveStop,
            CurrentTrailingActivated: currentPositionState.TrailingActivated,
            IsLongOpenToday: latestResult.Action == StrategyAction.EnterLong,
            IsLongExitToday: latestResult.Action is StrategyAction.ExitLongByStop or StrategyAction.ExitLongByEmaCross,
            LatestReason: latestResult.Reason,
            Error: null);
    }

    private static decimal? CalculateClosedTradeProfitPercent(StrategyEvaluationResult result)
    {
        if (result.EntryPrice is null || result.ExecutionPrice is null)
            return null;

        return CalculateProfitPercent(result.EntryPrice.Value, result.ExecutionPrice.Value);
    }

    private static decimal CalculateOpenProfitPercent(
        StrategyEvaluationResult latestResult,
        decimal currentPrice)
    {
        if (latestResult.PositionStateAfter.Side != PositionSide.Long ||
            latestResult.PositionStateAfter.EntryPrice is null)
        {
            return 0m;
        }

        return CalculateProfitPercent(
            latestResult.PositionStateAfter.EntryPrice.Value,
            currentPrice);
    }

    private static decimal CalculateProfitPercent(decimal startPrice, decimal endPrice)
    {
        if (startPrice == 0m)
            return 0m;

        return ((endPrice - startPrice) / startPrice) * 100m;
    }

    private void LogStockResult(
        NasdaqStrategyReportRow row,
        int current,
        int total)
    {
        // Enhanced logging with terminal-friendly format and visual separation
        if (!string.IsNullOrWhiteSpace(row.Error))
        {
            var message = new StringBuilder();
            message.AppendLine();
            message.AppendLine(new string('=', 70));
            message.AppendLine($"[FAILED] [{current}/{total}] - {row.Symbol}  ({row.SecurityName})");
            message.AppendLine(new string('-', 70));
            message.AppendLine($"Error: {row.Error}");
            message.AppendLine(new string('=', 70));
            _logger.LogWarning(message.ToString());
            return;
        }

        var output = new StringBuilder();
        output.AppendLine();
        output.AppendLine(new string('=', 70));
        output.AppendLine($"[{current}/{total}]  {row.Symbol}   \"{row.SecurityName}\"");
        output.AppendLine(new string('-', 70));

        output.AppendLine($"  Total PnL:          {FormatPercent(row.TotalStrategyProfitPercent),7}%   |    Buy&Hold: {FormatPercent(row.BuyAndHoldProfitPercent),7}%");
        output.AppendLine($"  Realized PnL:       {FormatPercent(row.RealizedStrategyProfitPercent),7}%   |    Open:     {FormatPercent(row.OpenStrategyProfitPercent),7}%");
        output.AppendLine($"  Trades:             {row.TradeCount,4}   |   Wins: {row.WinningTradeCount,4}   |   Losses: {row.LosingTradeCount,4}");
        output.AppendLine($"  State:              {row.CurrentSide}/{row.CurrentAction}");
        output.AppendLine($"  Entry Date:         {FormatDate(row.CurrentEntryDate)}");
        output.AppendLine($"  Entry Price:        {FormatDecimal(row.CurrentEntryPrice)}");
        output.AppendLine($"  Active Stop:        {FormatDecimal(row.CurrentActiveStop)}");
        output.AppendLine($"  Trailing Activated: {row.CurrentTrailingActivated}");
        output.AppendLine($"  Is Long Open Today: {row.IsLongOpenToday}");
        output.AppendLine($"  Is Long Exit Today: {row.IsLongExitToday}");
        if (!string.IsNullOrWhiteSpace(row.LatestReason))
            output.AppendLine($"  Latest Reason:      {row.LatestReason}");

        output.AppendLine(new string('=', 70));
        _logger.LogInformation(output.ToString());
    }

    private static bool IsProfitableStrategyResult(NasdaqStrategyReportRow row)
    {
        return string.IsNullOrWhiteSpace(row.Error)
               && row.TotalStrategyProfitPercent > 0m;
    }

    private static async Task<string> WriteCsv(
        IReadOnlyList<NasdaqStrategyReportRow> rows,
        string reportDirectory,
        string fileName,
        CancellationToken ct)
    {
        Directory.CreateDirectory(reportDirectory);

        var reportPath = Path.Combine(
            reportDirectory,
            fileName);
        var csv = new StringBuilder();

        csv.AppendLine(string.Join(',', CsvHeaders));

        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(
                ',',
                Csv(row.Symbol),
                Csv(row.SecurityName),
                Csv(row.EvaluationStartDate),
                Csv(row.EvaluationEndDate),
                Csv(row.FirstCandleDate),
                Csv(row.LatestCandleDate),
                Csv(row.OverallStartPrice),
                Csv(row.CurrentPrice),
                Csv(row.BuyAndHoldProfitPercent),
                Csv(row.RealizedStrategyProfitPercent),
                Csv(row.OpenStrategyProfitPercent),
                Csv(row.TotalStrategyProfitPercent),
                Csv(row.TradeCount),
                Csv(row.WinningTradeCount),
                Csv(row.LosingTradeCount),
                Csv(row.CurrentAction),
                Csv(row.CurrentSide),
                Csv(row.CurrentEntryDate),
                Csv(row.CurrentEntryPrice),
                Csv(row.CurrentActiveStop),
                Csv(row.CurrentTrailingActivated),
                Csv(row.IsLongOpenToday),
                Csv(row.IsLongExitToday),
                Csv(row.LatestReason),
                Csv(row.Error)));
        }

        await File.WriteAllTextAsync(reportPath, csv.ToString(), Encoding.UTF8, ct);

        return reportPath;
    }

    private static string GetReportDirectory()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "reports",
            "nasdaq-strategy-evaluations");
    }

    private static TradeStrategy CreateStrategy()
    {
        return new TradeStrategy(
            Guid.NewGuid(),
            Timeframe.Daily,
            new EmaTrendSettings(FastEmaPeriod, SlowEmaPeriod),
            new AdxTrendStrengthSettings(AdxPeriod, AdxThreshold, AdxNonFallingLookBackBars),
            new AtrStopSettings(
                AtrPeriod,
                AtrInitialStopMultiplier,
                AtrTrailingStopMultiplier,
                AtrTrailingActivationMultiplier));
    }

    private static StrategyMonitor CreateMonitor(string symbol)
    {
        return new StrategyMonitor(
            Guid.NewGuid(),
            tradeStrategyId: 1,
            Guid.NewGuid(),
            symbol,
            Timeframe.Daily,
            DateTime.UtcNow);
    }

    private static string FormatPercent(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "-";
    }

    private static string FormatDate(DateOnly? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-";
    }

    private static string Csv(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            bool boolean => boolean ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };

        if (text.Contains('"'))
            text = text.Replace("\"", "\"\"");

        if (text.Contains(',') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            text = $"\"{text}\"";

        return text;
    }

    private static DateTimeOffset GetNextWeekdayRunUtc(
        DateTimeOffset nowUtc,
        TimeZoneInfo easternTimeZone)
    {
        var easternNow = TimeZoneInfo.ConvertTime(nowUtc, easternTimeZone);
        var runDate = DateOnly.FromDateTime(easternNow.Date);
        var todayRunLocal = ToLocalDateTime(runDate, RunAtEasternTime);

        if (easternNow.DateTime >= todayRunLocal)
            runDate = runDate.AddDays(1);

        while (IsWeekend(runDate.DayOfWeek))
        {
            runDate = runDate.AddDays(1);
        }

        var nextRunLocal = ToLocalDateTime(runDate, RunAtEasternTime);
        var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(
            nextRunLocal,
            easternTimeZone);

        return new DateTimeOffset(nextRunUtc, TimeSpan.Zero);
    }

    private static DateTime ToLocalDateTime(DateOnly date, TimeOnly time)
    {
        return new DateTime(
            date.Year,
            date.Month,
            date.Day,
            time.Hour,
            time.Minute,
            time.Second,
            DateTimeKind.Unspecified);
    }

    private static bool IsWeekend(DayOfWeek day)
    {
        return day is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }

    private static TimeZoneInfo GetEasternTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }
    }

    private static readonly string[] CsvHeaders =
    [
        "symbol",
        "security_name",
        "evaluation_start_date",
        "evaluation_end_date",
        "first_candle_date",
        "latest_candle_date",
        "overall_start_price",
        "current_price",
        "buy_and_hold_profit_percent",
        "realized_strategy_profit_percent",
        "open_strategy_profit_percent",
        "total_strategy_profit_percent",
        "trade_count",
        "winning_trade_count",
        "losing_trade_count",
        "current_action",
        "current_side",
        "current_entry_date",
        "current_entry_price",
        "current_active_stop",
        "current_trailing_activated",
        "is_long_open_today",
        "is_long_exit_today",
        "latest_reason",
        "error"
    ];

    private sealed record NasdaqStrategyReportRow(
        string Symbol,
        string SecurityName,
        DateOnly? EvaluationStartDate,
        DateOnly? EvaluationEndDate,
        DateOnly? FirstCandleDate,
        DateOnly? LatestCandleDate,
        decimal? OverallStartPrice,
        decimal? CurrentPrice,
        decimal BuyAndHoldProfitPercent,
        decimal RealizedStrategyProfitPercent,
        decimal OpenStrategyProfitPercent,
        decimal TotalStrategyProfitPercent,
        int TradeCount,
        int WinningTradeCount,
        int LosingTradeCount,
        string CurrentAction,
        string CurrentSide,
        DateOnly? CurrentEntryDate,
        decimal? CurrentEntryPrice,
        decimal? CurrentActiveStop,
        bool CurrentTrailingActivated,
        bool IsLongOpenToday,
        bool IsLongExitToday,
        string LatestReason,
        string? Error)
    {
        public static NasdaqStrategyReportRow Failed(
            NasdaqListedStock stock,
            DateOnly? evaluationStartDate,
            DateOnly? evaluationEndDate,
            string error)
        {
            return new NasdaqStrategyReportRow(
                Symbol: stock.Symbol,
                SecurityName: stock.SecurityName,
                EvaluationStartDate: evaluationStartDate,
                EvaluationEndDate: evaluationEndDate,
                FirstCandleDate: null,
                LatestCandleDate: null,
                OverallStartPrice: null,
                CurrentPrice: null,
                BuyAndHoldProfitPercent: 0m,
                RealizedStrategyProfitPercent: 0m,
                OpenStrategyProfitPercent: 0m,
                TotalStrategyProfitPercent: 0m,
                TradeCount: 0,
                WinningTradeCount: 0,
                LosingTradeCount: 0,
                CurrentAction: string.Empty,
                CurrentSide: string.Empty,
                CurrentEntryDate: null,
                CurrentEntryPrice: null,
                CurrentActiveStop: null,
                CurrentTrailingActivated: false,
                IsLongOpenToday: false,
                IsLongExitToday: false,
                LatestReason: string.Empty,
                Error: error);
        }
    }
}
