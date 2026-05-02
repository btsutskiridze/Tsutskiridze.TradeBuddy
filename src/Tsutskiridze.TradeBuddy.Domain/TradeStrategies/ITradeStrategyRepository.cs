namespace Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

public interface ITradeStrategyRepository
{
    Task<TradeStrategy> AddAsync(TradeStrategy strategy, CancellationToken ct = default);
    Task<TradeStrategy?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<TradeStrategy>> ListByChatIdAsync(Guid chatId, CancellationToken ct = default);
    void Remove(TradeStrategy strategy);
}
