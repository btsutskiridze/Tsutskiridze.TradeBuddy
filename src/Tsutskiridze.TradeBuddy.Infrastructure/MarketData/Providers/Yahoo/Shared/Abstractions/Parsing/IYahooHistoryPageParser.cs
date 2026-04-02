using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions.Parsing;

public interface IYahooHistoryPageParser
{
    List<StockDayPriceDto> Parse(YahooPageContext pageContext);
}

