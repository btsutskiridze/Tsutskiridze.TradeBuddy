using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Queries.GetStockNews;

public sealed record GetStockNewsQuery(
    string Symbol,
    IReadOnlyCollection<NewsSource>? Sources = null,
    int? Limit = null,
    DateTime? From = null,
    DateTime? To = null,
    SortType SortType = SortType.New) : IQuery<StockNewsDto>;

public sealed class GetStockNewsQueryHandler : IQueryHandler<GetStockNewsQuery, StockNewsDto>
{
    private readonly IStockNewsReader _newsReader;

    public GetStockNewsQueryHandler(IStockNewsReader newsReader)
    {
        _newsReader = newsReader;
    }

    public async ValueTask<StockNewsDto> Handle(GetStockNewsQuery query, CancellationToken cancellationToken)
    {
        return await _newsReader.GetNewsAsync(
            query.Symbol,
            query.Sources,
            query.Limit,
            query.From,
            query.To,
            query.SortType,
            cancellationToken);
    }
}