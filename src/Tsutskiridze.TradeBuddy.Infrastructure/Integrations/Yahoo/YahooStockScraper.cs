using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.DTOs.MarketData;
using Tsutskiridze.TradeBuddy.Application.DTOs.Providers.MarketData;
using Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo.Utilities;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Integrations.Yahoo
{
    public class YahooStockScraper : YahooScraperBase, IYahooMarketDataProvider
    {
        public YahooStockScraper(
            HttpClient httpClient,
            ILogger<YahooScraperBase> logger,
            IYahooCookieBypassService yahooCookieBypassService
        ) : base(httpClient, logger, yahooCookieBypassService)
        {
        }

        public async Task<bool> StockSymbolExits(string symbol)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}");

            var nameNode = document.DocumentNode.SelectSingleNode("//h1[contains(@class, 'yf-4vbjci')]");

            return nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText.Trim());
        }

        public async Task<List<StockDayPriceDto>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}/history");

            var tableNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'table')]");
            if (tableNode == null)
            {
                return new List<StockDayPriceDto>();
            }

            var stockDayPrices = new List<StockDayPriceDto>();

            var rows = tableNode.SelectNodes(".//tbody/tr");
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    try
                    {
                        var cells = row.SelectNodes(".//td");

                        if (cells == null || cells.Count < 7 ||
                            !string.IsNullOrEmpty(cells[0].GetAttributeValue("colspan", "")))
                        {
                            continue;
                        }

                        var stockPrice = new StockDayPriceDto
                        {
                            Date = DateTime.Parse(cells[0].InnerText.Trim()).ToString("yyyy-MM-dd"),
                            Open = cells[1].InnerText.Trim(),
                            High = cells[2].InnerText.Trim(),
                            Low = cells[3].InnerText.Trim(),
                            Close = cells[4].InnerText.Trim(),
                            Volume = cells[6].InnerText.Trim()
                        };

                        stockDayPrices.Add(stockPrice);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing a stock price row.");
                    }
                }
            }

            if (days.HasValue && days.Value > 0)
            {
                stockDayPrices = stockDayPrices.Take(days.Value).ToList();
            }

            return stockDayPrices;
        }

        public async Task<StockOverviewDto?> GetStockOverview(string symbol)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}/key-statistics");

            var overview = new StockOverviewDto();

            try
            {
                var roeNode = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Management Effectiveness')]//tr[td[contains(text(), 'Return on Equity')]]/td[contains(@class, 'value')]");
                overview.ReturnOnEquityTTM = roeNode?.InnerText.Trim() ?? "";

                var psRow = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Valuation Measures')]//tr[td[contains(text(), 'Price/Sales')]]");
                if (psRow != null)
                {
                    var psCells = psRow.SelectNodes("td");
                    if (psCells != null && psCells.Count >= 2)
                    {
                        overview.PriceToSalesRatioTTM = psCells[1].InnerText.Trim();
                    }
                }

                var qrgNode = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Income Statement')]//tr[td[contains(text(), 'Quarterly Revenue Growth')]]/td[contains(@class, 'value')]");
                overview.QuarterlyRevenueGrowthYOY = qrgNode?.InnerText.Trim() ?? "";

                var ma50Node = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Trading Information')]//tr[td[contains(., '50-Day Moving Average')]]/td[contains(@class, 'value')]");
                overview.PriceAvg50 = ma50Node?.InnerText.Trim() ?? "";

                var ma200Node = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Trading Information')]//tr[td[contains(., '200-Day Moving Average')]]/td[contains(@class, 'value')]");
                overview.PriceAvg200 = ma200Node?.InnerText.Trim() ?? "";

                var soNode = document.DocumentNode.SelectSingleNode(
                    "//section[contains(., 'Share Statistics')]//tr[td[contains(., 'Shares Outstanding')]]/td[contains(@class, 'value')]");
                overview.SharesOutstanding = soNode?.InnerText.Trim() ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing stock overview data.");
            }

            return overview;
        }

        public async Task<AnnualReportDto?> GetStockLastAnnualReport(string symbol)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}/financials");

            var report = new AnnualReportDto();

            try
            {
                var fiscalDateNode = document.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'tableHeader')]//div[@data-cpos='1']");
                report.FiscalDateEnding = fiscalDateNode != null ?
                    DateTime.Parse(fiscalDateNode.InnerText.Trim()).ToString("yyyy-MM-dd") : "";

                report.ReportedCurrency = "USD";

                string? ExtractValueByRowLabel(string label)
                {
                    var row = document.DocumentNode.SelectSingleNode(
                        $"//div[contains(@class, 'tableBody')]//div[contains(@class, 'row')][.//div[contains(@class, 'rowTitle') and contains(normalize-space(.), '{label}')]]");
                    if (row != null)
                    {
                        var columns = row.SelectNodes(".//div[contains(@class, 'column')]");
                        if (columns != null && columns.Count >= 3)
                        {
                            return columns[2].InnerText.Trim();
                        }
                    }
                    return null;
                }

                report.TotalRevenue = ExtractValueByRowLabel("Total Revenue") ?? "";
                report.CostOfRevenue = ExtractValueByRowLabel("Cost of Revenue") ?? "";
                report.GrossProfit = ExtractValueByRowLabel("Gross Profit") ?? "";
                report.OperatingIncome = ExtractValueByRowLabel("Operating Income") ?? "";
                report.NetIncome = ExtractValueByRowLabel("Net Income") ?? ExtractValueByRowLabel("Net Income Common Stockholders") ?? "";
                report.DepreciationAndAmortization = ExtractValueByRowLabel("Reconciled Depreciation") ?? "";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing annual report data.");
            }

            return report;
        }

        public async Task<StockQuoteDto?> GetStockQuote(string symbol)
        {
            var document = await GetHtmlDocumentAsync($"quote/{symbol}");

            var stockQuote = new StockQuoteDto { Symbol = symbol };

            try
            {
                var nameNode = document.DocumentNode.SelectSingleNode("//h1[contains(@class, 'yf-4vbjci')]");
                stockQuote.Name = nameNode?.InnerText.Trim() ?? "";

                var exchangeNode = document.DocumentNode.SelectSingleNode("//span[contains(@class, 'exchange')]");
                if (exchangeNode != null)
                {
                    var exchangeSpan = exchangeNode.SelectSingleNode(".//span[1]");
                    var currencySpan = exchangeNode.SelectSingleNode(".//span[3]");
                    stockQuote.Exchange = exchangeSpan?.InnerText.Trim() ?? exchangeNode.InnerText.Trim();
                    stockQuote.Currency = currencySpan?.InnerText.Trim() ?? "n/a";
                }

                var priceNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price']");
                stockQuote.Price = priceNode?.InnerText.Trim() ?? "";

                var changeNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price-change']");
                stockQuote.Change = changeNode?.InnerText.Trim() ?? "";

                var changePercentNode = document.DocumentNode.SelectSingleNode("//span[@data-testid='qsp-price-change-percent']");
                if (changePercentNode != null)
                {
                    stockQuote.ChangesPercentage = changePercentNode.InnerText
                        .Replace("(", "")
                        .Replace(")", "")
                        .Trim();
                }

                var timestampNode = document.DocumentNode.SelectSingleNode("//div[@slot='marketTimeNotice']//span[contains(@class, 'yf-ipw1h0')]");
                stockQuote.Timestamp = timestampNode?.InnerText.Trim() ?? "";

                var prevCloseNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Previous Close']]//fin-streamer");
                stockQuote.PreviousClose = prevCloseNode?.GetAttributeValue("data-value", "0") ?? "";

                var openNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Open']]//fin-streamer");
                stockQuote.Open = openNode?.GetAttributeValue("data-value", "0") ?? "";

                var dayRangeNode = document.DocumentNode.SelectSingleNode("//fin-streamer[@data-field='regularMarketDayRange']");
                if (dayRangeNode != null)
                {
                    string[] parts = dayRangeNode.InnerText.Split('-');
                    if (parts.Length == 2)
                    {
                        stockQuote.DayLow = parts[0].Trim();
                        stockQuote.DayHigh = parts[1].Trim();
                    }
                }

                var weekRangeNode = document.DocumentNode.SelectSingleNode("//fin-streamer[@data-field='fiftyTwoWeekRange']");
                if (weekRangeNode != null)
                {
                    string[] parts = weekRangeNode.InnerText.Split('-');
                    if (parts.Length == 2)
                    {
                        stockQuote.YearLow = parts[0].Trim();
                        stockQuote.YearHigh = parts[1].Trim();
                    }
                }

                var marketCapNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Market Cap (intraday)']]//fin-streamer");
                stockQuote.MarketCap = marketCapNode?.InnerText.Trim() ?? "";

                var volumeNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Volume']]//fin-streamer");
                stockQuote.Volume = volumeNode?.InnerText.Replace(",", "").Trim() ?? "";

                var avgVolumeNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Avg. Volume']]//fin-streamer");
                stockQuote.AvgVolume = avgVolumeNode?.InnerText.Replace(",", "").Trim() ?? "";

                var epsNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='EPS (TTM)']]//fin-streamer");
                stockQuote.Eps = epsNode?.InnerText.Trim() ?? "";

                var peNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='PE Ratio (TTM)']]//fin-streamer");
                if (peNode != null)
                {
                    string peText = peNode.InnerText.Trim();
                    stockQuote.Pe = peText == "--" || string.IsNullOrWhiteSpace(peText) ? null : peText;
                }

                var earningsNode = document.DocumentNode.SelectSingleNode("//li[.//span[@title='Earnings Date']]");
                if (earningsNode != null)
                {
                    var earningsValueNode = earningsNode.SelectSingleNode(".//span[contains(@class, 'value')]");
                    stockQuote.EarningsAnnouncement = earningsValueNode?.InnerText.Trim() ??
                        earningsNode.InnerText.Replace("Earnings Date", "").Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing stock quote data.");
            }

            return stockQuote;
        }
    }
}
