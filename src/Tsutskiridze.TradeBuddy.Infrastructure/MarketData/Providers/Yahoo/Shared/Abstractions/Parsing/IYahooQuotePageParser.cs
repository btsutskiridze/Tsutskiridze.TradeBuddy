using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;

namespace Tsutskiridze.TradeBuddy.Infrastructure.MarketData.Providers.Yahoo.Shared.Abstractions.Parsing;

public interface IYahooQuotePageParser
{
    bool StockSymbolExists(YahooPageContext pageContext);
    StockQuoteDto? Parse(YahooPageContext pageContext);
}

