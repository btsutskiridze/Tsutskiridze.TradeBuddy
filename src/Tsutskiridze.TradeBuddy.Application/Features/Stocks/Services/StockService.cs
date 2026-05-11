using SharedKernel.Data;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Persistence;
using Tsutskiridze.TradeBuddy.Application.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Application.Features.Stocks.Specifications;
using Tsutskiridze.TradeBuddy.Domain.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Services;

internal class StockService : IStockService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Stock> _stocks;
    private readonly IDbExceptionClassifier _persistenceExceptionClassifier;

    public StockService(IRepository<Stock> stocks, IDbExceptionClassifier persistenceExceptionClassifier,
        IUnitOfWork uow)
    {
        _stocks = stocks;
        _persistenceExceptionClassifier = persistenceExceptionClassifier;
        _uow = uow;
    }

    public async Task<Stock> GetOrCreateStock(string symbol, string name, string currency,
        CancellationToken ct = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var stock = await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(normalizedSymbol), ct);
        if (stock is not null)
            return stock;
        try
        {
            stock = new Stock(Guid.NewGuid(), normalizedSymbol, currency, name);
            await _stocks.AddAsync(stock, ct);
            await _uow.SaveChangesAsync(ct);
            return stock;
        }
        catch (Exception ex) when (
            _persistenceExceptionClassifier.TryClassify(ex, out var error))
        {
            if (error.Code == PersistenceErrorCode.DuplicateStockSymbol)
            {
                return await _stocks.FirstOrDefaultAsync(new StockBySymbolSpec(normalizedSymbol), ct)
                       ?? throw new ApplicationLayerException("Failed to get stock");
            }

            throw;
        }
    }
}