using Mediator;
using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Google;

public record GoogleNewsQuery(string Symbol, int? Limit = null) : IQuery<List<GoogleNewsItemDto>?>;

public class GoogleNewsQueryHandler : IQueryHandler<GoogleNewsQuery, List<GoogleNewsItemDto>?>
{
    private readonly INewsProvider _news;

    public GoogleNewsQueryHandler(INewsProvider news)
    {
        _news = news;
    }

    public async ValueTask<List<GoogleNewsItemDto>?> Handle(
        GoogleNewsQuery query,
        CancellationToken cancellationToken)
    {
        return await _news.GetGoogleNews(query.Symbol, query.Limit);
    }
}
