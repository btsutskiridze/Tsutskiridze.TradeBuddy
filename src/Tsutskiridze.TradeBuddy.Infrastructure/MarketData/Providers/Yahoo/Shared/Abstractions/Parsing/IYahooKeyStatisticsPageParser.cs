using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions.Parsing;

public interface IYahooKeyStatisticsPageParser
{
    StockOverviewDto Parse(YahooPageContext pageContext);
}

