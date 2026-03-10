using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Yahoo;

public record YahooNewsQuery(string Symbol, int? Limit = null) : IQuery<List<YahooNewsItemDto>?>;

public class YahooNewsQueryHandler : IQueryHandler<YahooNewsQuery, List<YahooNewsItemDto>?>
{
    private readonly INewsAggregator _news;

    public YahooNewsQueryHandler(INewsAggregator news)
    {
        _news = news;
    }

    public async ValueTask<List<YahooNewsItemDto>?> Handle(
        YahooNewsQuery query,
        CancellationToken cancellationToken)
    {
        return await _news.GetYahooNews(query.Symbol, query.Limit);
    }
}
