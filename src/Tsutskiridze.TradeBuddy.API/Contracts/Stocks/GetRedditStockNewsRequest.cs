using Tsutskiridze.TradeBuddy.Application.Common.Enums;

namespace Tsutskiridze.TradeBuddy.API.Contracts.Stocks;

public class GetRedditStockNewsRequest
{
    public int Limit { get; init; } = 10;
    
    public string SortType { get; init; } = nameof(Application.Common.Enums.SortType.New);

    public SortType ToSortType()
    {
        return Enum.TryParse<SortType>(SortType, true, out var sortType)
            ? sortType
            : Application.Common.Enums.SortType.New;
    }
}