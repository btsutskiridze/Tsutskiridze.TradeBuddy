using Tsutskiridze.TradeBuddy.Application.Abstractions.MarketData.Models;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

public interface IYahooFinancialsPageParser
{
    AnnualReport? Parse(YahooPageContext pageContext);
}

