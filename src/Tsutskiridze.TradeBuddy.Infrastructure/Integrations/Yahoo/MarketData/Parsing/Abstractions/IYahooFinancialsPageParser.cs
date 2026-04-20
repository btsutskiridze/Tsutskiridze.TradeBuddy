using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Common.Models;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.MarketData.Parsing.Abstractions;

public interface IYahooFinancialsPageParser
{
    AnnualReportDto? Parse(YahooPageContext pageContext);
}

