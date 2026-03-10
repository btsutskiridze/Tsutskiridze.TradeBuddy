using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Application.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Reddit;

public record RedditNewsQuery(
    string Symbol,
    RedditSortType Sort = RedditSortType.New,
    int? Limit = null) : IQuery<List<RedditPostDto>?>;

public class RedditNewsQueryHandler : IQueryHandler<RedditNewsQuery, List<RedditPostDto>?>
{
    private readonly INewsAggregator _news;

    public RedditNewsQueryHandler(INewsAggregator news)
    {
        _news = news;
    }

    public async ValueTask<List<RedditPostDto>?> Handle(
        RedditNewsQuery query,
        CancellationToken cancellationToken)
    {
        return await _news.GetRedditNews(query.Symbol, query.Sort, query.Limit);
    }
}
