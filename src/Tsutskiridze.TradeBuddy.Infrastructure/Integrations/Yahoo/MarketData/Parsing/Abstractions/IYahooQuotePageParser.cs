using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

public interface IYahooQuotePageParser
{
    bool StockSymbolExists(YahooPageContext pageContext);
    StockQuoteDto? Parse(YahooPageContext pageContext);
}

