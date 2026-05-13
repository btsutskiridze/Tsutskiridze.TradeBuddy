using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.EvaluateDailyStrategyMonitors;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Jobs;

public sealed class DailyStrategyMonitorJob:BackgroundService
{
private static readonly TimeOnly RunAtEasternTime = new(17, 0); // 5:00 PM ET

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyStrategyMonitorJob> _logger;

    public DailyStrategyMonitorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyStrategyMonitorJob> logger)
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
                await RunIfDueAsync(easternTimeZone, ct);
                
                var nowUtc = DateTimeOffset.UtcNow;
                var nextRunUtc = GetNextWeekdayRunUtc(nowUtc, easternTimeZone);

                var delay = nextRunUtc - nowUtc;

                _logger.LogInformation(
                    "Daily strategy job next run: {NextRunUtc} UTC",
                    nextRunUtc);

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, ct);
                }

                var easternNow = TimeZoneInfo.ConvertTime(
                    DateTimeOffset.UtcNow,
                    easternTimeZone);

                if (IsWeekend(easternNow.DayOfWeek))
                {
                    continue;
                }

                var tradingDate = DateOnly.FromDateTime(easternNow.Date);
                
                await EvaluateStrategyMonitors(tradingDate, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Daily strategy job failed");

                // Avoid tight loop if something keeps failing.
                await Task.Delay(TimeSpan.FromMinutes(5), ct);
            }
        }
    }

    private async Task RunIfDueAsync(
        TimeZoneInfo easternTimeZone,
        CancellationToken ct)
    {
        var easternNow = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            easternTimeZone);

        if (IsWeekend(easternNow.DayOfWeek))
            return;

        var currentTime = TimeOnly.FromDateTime(easternNow.DateTime);

        if (currentTime < RunAtEasternTime)
            return;

        var tradingDate = DateOnly.FromDateTime(easternNow.Date);

        await EvaluateStrategyMonitors(tradingDate, ct);
    }

    private async Task EvaluateStrategyMonitors(DateOnly tradingDate, CancellationToken ct)
    {
        _logger.LogInformation(
            "Running daily strategy job for {TradingDate}",
            tradingDate);
        
        await using var scope = _scopeFactory.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(
            new EvaluateStrategyMonitorsCommand(tradingDate),
            ct);
        
        _logger.LogInformation(
            "Daily strategy job completed for {TradingDate}",
            tradingDate);
    }

    private static DateTimeOffset GetNextWeekdayRunUtc(
        DateTimeOffset nowUtc,
        TimeZoneInfo easternTimeZone)
    {
        var easternNow = TimeZoneInfo.ConvertTime(nowUtc, easternTimeZone);

        var runDate = DateOnly.FromDateTime(easternNow.Date);

        var todayRunLocal = ToLocalDateTime(runDate, RunAtEasternTime);

        if (easternNow.DateTime >= todayRunLocal)
        {
            runDate = runDate.AddDays(1);
        }

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
}