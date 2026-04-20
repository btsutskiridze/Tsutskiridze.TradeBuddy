using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries;

public record StockNewsQuery(string Symbol, int? Limit = null) : IQuery<StockNewsDto>;

public class StockNewsQueryHandler : IQueryHandler<StockNewsQuery, StockNewsDto>
{
    private readonly INewsProvider _news;

    public StockNewsQueryHandler(INewsProvider news)
    {
        _news = news;
    }

    public async ValueTask<StockNewsDto> Handle(StockNewsQuery query, CancellationToken cancellationToken)
    {
        return await _news.GetAllNews(query.Symbol, query.Limit);
    }
}