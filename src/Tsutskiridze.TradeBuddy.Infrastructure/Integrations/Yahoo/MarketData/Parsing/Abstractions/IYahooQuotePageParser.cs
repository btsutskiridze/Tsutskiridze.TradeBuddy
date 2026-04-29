using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

public interface IYahooQuotePageParser
{
    bool StockSymbolExists(YahooPageContext pageContext);
    StockQuote? Parse(YahooPageContext pageContext);
}

