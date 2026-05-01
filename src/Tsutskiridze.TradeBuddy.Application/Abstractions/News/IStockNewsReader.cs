using Tsutskiridze.TradeBuddy.Application.Common.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Abstractions.News;

public interface IStockNewsReader
{
    Task<StockNews> GetNewsAsync(
        string symbol,
        IReadOnlyCollection<NewsSource>? sources = null,
        int? limit = null,
        DateTime? from = null,
        DateTime? to = null,
        SortType sortType = SortType.New,
        CancellationToken ct = default);
}
