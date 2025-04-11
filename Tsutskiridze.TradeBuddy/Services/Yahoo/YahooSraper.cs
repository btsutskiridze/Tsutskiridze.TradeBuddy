using HtmlAgilityPack;
using Tsutskiridze.TradeBuddy.DTOs.Yahoo;
using Tsutskiridze.TradeBuddy.Models.AlphaVantage;

namespace Tsutskiridze.TradeBuddy.Services.Yahoo
{
    public class YahooSraper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<YahooSraper> _logger;

        public YahooSraper(HttpClient httpClient, ILogger<YahooSraper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<YahooNews>?> GetNewsAsync(string symbol, int? limit = null)
        {
            var response = await _httpClient.GetAsync($"quote/{symbol}/news?lang=en-US&region=US");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> formData;

            try
            {
                formData = YahooCookieAvoider.ExtractFormData(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                formData = null;
            }

            if (formData != null)
            {
                try
                {
                    string pageContent = await YahooCookieAvoider.SubmitFormAsync(formData);
                    html = pageContent;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                }
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var newsNodes = htmlDocument.DocumentNode.SelectNodes("//section[@data-testid='storyitem']");

            if (newsNodes == null)
            {
                return null;
            }

            _logger.LogInformation("Scraping Yahoo news for {Symbol}", symbol);

            var newsList = new List<YahooNews>();

            foreach (var node in newsNodes)
            {
                try
                {
                    var titleNode = node.SelectSingleNode(".//h3");
                    var urlNode = node.SelectSingleNode(".//a[@class='subtle-link']");
                    var summaryNode = node.SelectSingleNode(".//p");
                    var timeNode = node.SelectSingleNode(".//div[contains(@class, 'publishing')]");

                    string title = titleNode?.InnerText.Trim() ?? "N/A";
                    string newsUrl = urlNode?.GetAttributeValue("href", "").Trim() ?? "N/A";
                    if (!newsUrl.StartsWith("https")) newsUrl = "https://finance.yahoo.com" + newsUrl;
                    string summary = summaryNode?.InnerText.Trim() ?? "N/A";
                    string publishTime = timeNode?.InnerText.Trim().Split("• ").Last() ?? "N/A";

                    newsList.Add(new YahooNews
                    {
                        Title = title,
                        Url = newsUrl,
                        Summary = summary,
                        PublishTime = publishTime
                    });
                }
                catch (Exception)
                {
                }

                if (newsList.Count >= limit)
                {
                    break;
                }
            }

            return newsList.Count > 0 ? newsList : null;
        }


        public async Task<List<StockDayPrice>> GetStockPrevDaysClosePrices(string symbol, int? days = null)
        {
            // Call the endpoint to get the HTML content.
            var response = await _httpClient.GetAsync($"quote/{symbol}/history?lang=en-US&region=US");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> formData;

            try
            {
                // Attempt to extract the necessary form data to bypass Yahoo's cookie check.
                formData = YahooCookieAvoider.ExtractFormData(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                formData = null;
            }

            // If we got the form data successfully, then submit it to get the actual page content.
            if (formData != null)
            {
                try
                {
                    string pageContent = await YahooCookieAvoider.SubmitFormAsync(formData);
                    html = pageContent;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                }
            }

            // Load the HTML into an HtmlDocument for parsing.
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // Prepare a list to hold the parsed stock day prices.
            var stockDayPrices = new List<StockDayPrice>();

            // Locate the table in the document. This example assumes that the table has a class that includes 'table'.
            // Adjust the XPath if your structure is different.
            var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[contains(@class, 'table')]");
            if (tableNode != null)
            {
                // Select the rows within the tbody.
                var rows = tableNode.SelectNodes(".//tbody/tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        // Get all the td cells in this row.
                        var cells = row.SelectNodes(".//td");

                        // If cells are missing or the row is an event row (for example a dividend or stock split row), skip it.
                        // Event rows often include a colspan attribute, so we check for that.
                        if (cells == null || cells.Count < 7 ||
                            !string.IsNullOrEmpty(cells[0].GetAttributeValue("colspan", "")))
                        {
                            continue;
                        }

                        // Parse the expected data.
                        // Mapping: 
                        //   cells[0] -> Date,
                        //   cells[1] -> Open,
                        //   cells[2] -> High,
                        //   cells[3] -> Low,
                        //   cells[4] -> Close (we ignore the Adjusted Close in cells[5]),
                        //   cells[6] -> Volume.

                        var stockPrice = new StockDayPrice
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
                }
            }

            // If a days count was specified, take only the first `days` items.
            if (days.HasValue && days.Value > 0)
            {
                stockDayPrices = stockDayPrices.Take(days.Value).ToList();
            }

            return stockDayPrices;
        }

        public async Task<StockOverview?> GetStockOverview(string symbol)
        {
            // Call the endpoint to get the HTML content.
            var response = await _httpClient.GetAsync($"quote/{symbol}/key-statistics?lang=en-US&region=US");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> formData;

            try
            {
                // Attempt to extract the necessary form data to bypass Yahoo's cookie check.
                formData = YahooCookieAvoider.ExtractFormData(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                formData = null;
            }

            // If form data was extracted, then submit it to get the proper page content.
            if (formData != null)
            {
                try
                {
                    string pageContent = await YahooCookieAvoider.SubmitFormAsync(formData);
                    html = pageContent;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to submit form data to bypass cookie check");
                }
            }

            // Load the HTML into an HtmlDocument for parsing.
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            // Create a new StockOverview instance.
            var overview = new StockOverview();

            // 1. Extract Return On Equity (ttm)
            // This appears in the "Management Effectiveness" card.
            var roeNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//section[contains(., 'Management Effectiveness')]//tr[td[contains(text(), 'Return on Equity')]]/td[contains(@class, 'value')]");
            if (roeNode != null)
            {
                overview.ReturnOnEquityTTM = roeNode.InnerText.Trim();
            }
            else
            {
                _logger.LogWarning("Return on Equity node not found.");
            }

            // 2. Extract Price/Sales Ratio (ttm)
            // In the "Valuation Measures" section, the first value (Current column) for Price/Sales is used.
            var psRow = htmlDocument.DocumentNode.SelectSingleNode(
                "//section[contains(., 'Valuation Measures')]//tr[td[contains(text(), 'Price/Sales')]]");
            if (psRow != null)
            {
                // Assuming the first <td> is the label, the second is the "Current" value.
                var psCells = psRow.SelectNodes("td");
                if (psCells != null && psCells.Count >= 2)
                {
                    overview.PriceToSalesRatioTTM = psCells[1].InnerText.Trim();
                }
                else
                {
                    _logger.LogWarning("Price/Sales row does not have enough cells.");
                }
            }
            else
            {
                _logger.LogWarning("Price/Sales row not found.");
            }

            // 3. Extract Quarterly Revenue Growth YOY
            // Located in the "Income Statement" section.
            var qrgNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//section[contains(., 'Income Statement')]//tr[td[contains(text(), 'Quarterly Revenue Growth')]]/td[contains(@class, 'value')]");
            if (qrgNode != null)
            {
                overview.QuarterlyRevenueGrowthYOY = qrgNode.InnerText.Trim();
            }
            else
            {
                _logger.LogWarning("Quarterly Revenue Growth row not found.");
            }

            return overview;
        }

        public async Task<AnnualReport?> GetStockLastAnnualReport(string symbol)
        {
            var response = await _httpClient.GetAsync($"quote/{symbol}/financials?lang=en-US&region=US");
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> formData;

            try
            {
                formData = YahooCookieAvoider.ExtractFormData(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract Yahoo cookie form data");
                formData = null;
            }

            if (formData != null)
            {
                try
                {
                    string pageContent = await YahooCookieAvoider.SubmitFormAsync(formData);
                    html = pageContent;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to submit form data to bypass cookie check");
                }
            }

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var report = new AnnualReport();

            var fiscalDateNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class, 'tableHeader')]//div[@data-cpos='1']");
            if (fiscalDateNode != null)
            {
                report.FiscalDateEnding = DateTime.Parse(fiscalDateNode.InnerText.Trim()).ToString("yyyy-MM-dd");
            }
            else
            {
                _logger.LogWarning("Fiscal Date Ending not found in header.");
                report.FiscalDateEnding = "";
            }

            // 2. Set the reported currency.
            // This value may be found elsewhere in the page (for example in the quote header) but here we assign a default.
            report.ReportedCurrency = "USD";

            // Helper local function to extract a value for a given row label.
            string? ExtractValueByRowLabel(string label)
            {
                // Look for a row in the table body (the outer container has class "tableBody yf-9ft13")
                // where a descendant cell with class "rowTitle" contains the provided label.
                var row = htmlDocument.DocumentNode.SelectSingleNode(
                    $"//div[contains(@class, 'tableBody')]//div[contains(@class, 'row')][.//div[contains(@class, 'rowTitle') and contains(normalize-space(.), '{label}')]]");
                if (row != null)
                {
                    // Get all cells (div elements with class "column") for that row.
                    var columns = row.SelectNodes(".//div[contains(@class, 'column')]");
                    // In our layout the first column is the sticky label, the second column is TTM,
                    // and the third column (index 2) is the annual report value we want.
                    if (columns != null && columns.Count >= 3)
                    {
                        return columns[2].InnerText.Trim();
                    }
                    else
                    {
                        _logger.LogWarning($"Insufficient columns found for label: {label}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Row with label '{label}' not found.");
                }
                return null;
            }

            // 3. Extract each field from the corresponding row.
            report.TotalRevenue = ExtractValueByRowLabel("Total Revenue") ?? "";
            report.CostOfRevenue = ExtractValueByRowLabel("Cost of Revenue") ?? "";
            report.GrossProfit = ExtractValueByRowLabel("Gross Profit") ?? "";
            report.OperatingIncome = ExtractValueByRowLabel("Operating Income") ?? "";

            // For net income, the row might be labeled "Net Income Common Stockholders".
            report.NetIncome = ExtractValueByRowLabel("Net Income") ?? ExtractValueByRowLabel("Net Income Common Stockholders") ?? "";

            // For depreciation and amortization, we assume that the row "Reconciled Depreciation" contains that value.
            report.DepreciationAndAmortization = ExtractValueByRowLabel("Reconciled Depreciation") ?? "";

            return report;
        }

    }
}

