using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News.Mappings;

public static class MappingExtensions
{
    public static StockNewsDto.StockNewsItem ToDto(this YahooNewsItem source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        return new StockNewsDto.StockNewsItem
        {
            Title = source.Title,
            Summary = source.Summary,
            PublishTime = source.PublishTime,
            Source = NewsSource.Yahoo,
            Url = source.Url
        };
    }
}
