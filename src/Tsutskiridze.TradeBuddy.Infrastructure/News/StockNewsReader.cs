using Tsutskiridze.TradeBuddy.Application.Abstractions.News;
using Tsutskiridze.TradeBuddy.Application.Common.Enums;
using Tsutskiridze.TradeBuddy.Application.DTOs.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Common.Exceptions;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Mappings;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Finnhub.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.GoogleNews;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Mappings;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Reddit.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News.Mappings;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.News.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.News
{
    public sealed class StockNewsReader : IStockNewsReader
    {
        private readonly IFinnhubNewsProvider _finnhub;
        private readonly IRedditNewsProvider _reddit;
        private readonly IYahooNewsProvider _yahooProvider;
        private readonly IGoogleNewsProvider _googleProvider;

        public StockNewsReader(
            IFinnhubNewsProvider finnhub,
            IRedditNewsProvider reddit,
            IYahooNewsProvider yahooProvider,
            IGoogleNewsProvider googleProvider)
        {
            _finnhub = finnhub;
            _reddit = reddit;
            _yahooProvider = yahooProvider;
            _googleProvider = googleProvider;
        }

        public async Task<StockNewsDto> GetNewsAsync(
            string symbol,
            IReadOnlyCollection<NewsSource>? sources = null,
            int? limit = null,
            DateTime? from = null,
            DateTime? to = null,
            SortType sortType = SortType.New,
            CancellationToken ct = default)
        {
            var selectedSources = sources is { Count: > 0 }
                ? sources.ToHashSet()
                : Enum.GetValues<NewsSource>().ToHashSet();

            // Task<List<GoogleNewsItemDto>?> googleTask = selectedSources.Contains(NewsSource.Google)
            //     ? _googleProvider.GetNewsAsync(symbol, limit)
            //     : Task.FromResult<List<GoogleNewsItemDto>?>(null);

            Task<List<RedditPost>?> redditTask = selectedSources.Contains(NewsSource.Reddit)
                ? _reddit.GetRedditPosts(symbol, sortType, limit)
                : Task.FromResult<List<RedditPost>?>(null);

            Task<List<YahooNewsItem>?> yahooTask = selectedSources.Contains(NewsSource.Yahoo)
                ? _yahooProvider.GetNewsAsync(symbol, limit)
                : Task.FromResult<List<YahooNewsItem>?>(null);

            Task<List<FinnhubNewsItem>?> finnhubTask = selectedSources.Contains(NewsSource.Finnhub)
                ? _finnhub.GetCompanyNewsAsync(
                    symbol,
                    from ?? DateTime.UtcNow.AddDays(-7),
                    to ?? DateTime.UtcNow,
                    limit)
                : Task.FromResult<List<FinnhubNewsItem>?>(null);

            var allTasks = Task.WhenAll(redditTask, yahooTask, finnhubTask);

            try
            {
                await allTasks;
            }
            catch
            {
                throw new InfrastructureException(
                    "One or more news providers failed while fetching stock news.",
                    inner: allTasks.Exception!
                );
            }

            var items = new List<StockNewsDto.StockNewsItem>();

            if (redditTask.Result is not null)
                items.AddRange(redditTask.Result.Select(x => x.ToDto()));

            if (yahooTask.Result is not null)
                items.AddRange(yahooTask.Result.Select(x => x.ToDto()));

            if (finnhubTask.Result is not null)
                items.AddRange(finnhubTask.Result.Select(x => x.ToDto()));

            return new StockNewsDto()
            {
                Items = items
            };
        }
    }
}