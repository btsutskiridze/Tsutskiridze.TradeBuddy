using Mediator;
using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData;
using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Services;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Stocks;
using Tsutskiridze.TradeBuddy.Domain.StrategyEvaluation;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.Enums;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies.ValueObjects;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.ActivateStrategyMonitor;

public sealed record ActivateStrategyMonitorCommand(
    long ChatId,
    string StrategyCode,
    string Symbol
) : ICommand<StrategyMonitorEvaluationSummary>;

public class
    ActivateStrategyMonitorHandler : ICommandHandler<ActivateStrategyMonitorCommand, StrategyMonitorEvaluationSummary>
{
    private readonly IUnitOfWork _uow;
    private readonly IActiveChatProvider _activeChatProvider;
    private readonly IReadRepository<TradeStrategy, int> _strategies;
    private readonly IRepository<StrategyMonitor, int> _monitors;
    private readonly IRepository<Stock> _stocks;
    private readonly IMarketDataProvider _market;
    private readonly IEmaAdxAtrEvaluationCandleBuilder _emaAdxAtrEvaluationCandleBuilder;

    public ActivateStrategyMonitorHandler(
        IUnitOfWork uow,
        IActiveChatProvider activeChatProvider,
        IReadRepository<TradeStrategy, int> strategies,
        IRepository<StrategyMonitor, int> monitors,
        IRepository<Stock> stocks,
        IMarketDataProvider market,
        IEmaAdxAtrEvaluationCandleBuilder emaAdxAtrEvaluationCandleBuilder)
    {
        _uow = uow;
        _activeChatProvider = activeChatProvider;
        _strategies = strategies;
        _monitors = monitors;
        _stocks = stocks;
        _market = market;
        _emaAdxAtrEvaluationCandleBuilder = emaAdxAtrEvaluationCandleBuilder;
    }


    public async ValueTask<StrategyMonitorEvaluationSummary> Handle(ActivateStrategyMonitorCommand cmd,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var chatId = await _activeChatProvider.GetIdAsync(cmd.ChatId, ct);
        var (stockQuote, historyDateRange) = await FetchStockQuoteDetails(cmd.Symbol, ct);

        var stock = await GetOrCreateStock(cmd.Symbol, stockQuote.Name, stockQuote.Currency, ct);
        var strategy = await GetValidatedStrategyId(cmd.StrategyCode, chatId, ct);

        var strategyMonitor =
            await _monitors.FirstOrDefaultAsync(
                new StrategyMonitorByChatStrategyStockSpec(chatId, strategy.Id, stock.Id), ct);

        if (strategyMonitor is null)
        {
            strategyMonitor = new StrategyMonitor(
                chatId,
                strategy.Id,
                stock.Id,
                stock.Symbol,
                Timeframe.Daily,
                now
            );

            await _monitors.AddAsync(strategyMonitor, ct);
        }
        else
        {
            strategyMonitor.Activate();
        }

        var historyCandles = await _market.GetDailyCandles(
            cmd.Symbol,
            historyDateRange.From,
            historyDateRange.To,
            ct
        );
        var strategyCandles = _emaAdxAtrEvaluationCandleBuilder.BuildDailyStrategyCandles(
            strategy.EmaTrend.FastPeriod,
            strategy.EmaTrend.SlowPeriod,
            strategy.AdxTrendStrength.Period,
            strategy.AtrStop.Period,
            historyCandles
        );

        var results = EmaAdxAtrStrategyEvaluator.Replay(strategy, strategyMonitor, strategyCandles);
        var currentState = results[^1];
        
        strategyMonitor.MarkEvaluated(currentState.CandleDate);

        await _uow.SaveChangesAsync(ct);

        return StrategyMonitorEvaluationSummary.Create(
            cmd.ChatId,
            strategy.Code.ToString(),
            stock.Symbol,
            currentState);
    }

    private async Task<(StockQuote stockQuote, MarketHistoryDateRange historyDateRange)> FetchStockQuoteDetails(
        string symbol,
        CancellationToken ct)
    {
        var stockQuote = await _market.GetStockQuote(symbol, ct)
                         ?? throw new ResourceNotFoundException("Stock symbol not found");
        var historyDateRange = await _market.GetClosedDailyDateRange(symbol, ct);

        return (stockQuote, historyDateRange);
    }

    private async Task<TradeStrategy> GetValidatedStrategyId(string strategyCode, Guid chatId, CancellationToken ct)
    {
        var tradeStrategyCode = TradeStrategyCode.Parse(strategyCode);
        var strategy = await _strategies.FirstOrDefaultAsync(
            new ActiveTradeStrategyByChatIdAndIdSpec(chatId, tradeStrategyCode.Id),
            ct
        ) ?? throw new ResourceNotFoundException("Trade strategy not found.");

        return strategy;
    }

    private async Task<Stock> GetOrCreateStock(string symbol, string name, string currency, CancellationToken ct)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(normalizedSymbol), ct);
        if (stock is not null)
            return stock;

        stock = new Stock(Guid.NewGuid(), normalizedSymbol, currency, name);
        await _stocks.AddAsync(stock, ct);
        return stock;
    }
}