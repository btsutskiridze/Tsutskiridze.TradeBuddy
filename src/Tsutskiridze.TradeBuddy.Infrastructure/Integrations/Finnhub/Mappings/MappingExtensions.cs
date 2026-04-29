using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Mappings;

public static class MappingExtensions
{
    public static StockNews.StockNewsItem ToDto(this FinnhubNewsItem source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        return new StockNews.StockNewsItem
        {
            Title = source.Title,
            Summary = source.Summary,
            PublishTime = source.CreateTime,
            Source = NewsSource.Finnhub,
            Url = source.Url,
            Metadata = new Dictionary<string, string>
            {
                { nameof(FinnhubNewsItem.Category), source.Category }
            }
        };
    }
}
