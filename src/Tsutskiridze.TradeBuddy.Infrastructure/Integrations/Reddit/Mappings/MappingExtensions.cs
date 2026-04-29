using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Mappings;

public static class MappingExtensions
{
    public static StockNews.StockNewsItem ToDto(this RedditPost source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        return new StockNews.StockNewsItem
        {
            Title = source.Title,
            Summary = source.Body,
            PublishTime = source.CreateTime,
            Source = NewsSource.Reddit,
            Url = source.Url,
            Metadata = new Dictionary<string, string>
            {
                { nameof(RedditPost.Score), source.Score.ToString() },
                { nameof(RedditPost.CommentsCount), source.CommentsCount.ToString() }
            }
        };
    }
}
