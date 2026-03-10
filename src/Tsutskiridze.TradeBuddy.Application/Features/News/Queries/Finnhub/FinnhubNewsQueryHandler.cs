using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Finnhub;

public record FinnhubNewsQuery(
    string Symbol,
    DateTime? From = null,
    DateTime? To = null,
    int? Limit = null) : IQuery<List<FinnhubNewsItemDto>?>;

public class FinnhubNewsQueryHandler : IQueryHandler<FinnhubNewsQuery, List<FinnhubNewsItemDto>?>
{
    private readonly INewsAggregator _news;

    public FinnhubNewsQueryHandler(INewsAggregator news)
    {
        _news = news;
    }

    public async ValueTask<List<FinnhubNewsItemDto>?> Handle(
        FinnhubNewsQuery query,
        CancellationToken cancellationToken)
    {
        var from = query.From ?? DateTime.UtcNow.AddDays(-7);
        var to = query.To ?? DateTime.UtcNow;

        return await _news.GetFinnhubNews(query.Symbol, from, to, query.Limit);
    }
}