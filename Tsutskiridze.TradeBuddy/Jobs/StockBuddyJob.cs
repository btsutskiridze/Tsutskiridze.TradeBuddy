using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Services;
using Tsutskiridze.TradeBuddy.Services.AI;

namespace Tsutskiridze.TradeBuddy.Jobs
{
    public class StockBuddyJob : IJob
    {
        public string Name => "StockBuddyJob";
        public string Cron => "0 0 0 */1 * *"; // Every day at midnight
        public int Attempts => 1;
        public bool RunOnStart => true;

        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly StockPromptService _stockPromptService;
        private readonly IAIService _aiService;
        public StockBuddyJob(StockPromptService stockPromptService, IOptions<JsonSerializerOptions> jsonSerializerOptions, IAIService geminiService)
        {
            _stockPromptService = stockPromptService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
            _aiService = geminiService;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();

            var stockPrompt = await _stockPromptService.GetStockPrompt(Stocks.RCAT);

            Console.WriteLine("========================================");
            Console.WriteLine($"Stock Prompt: {JsonSerializer.Serialize(stockPrompt, _jsonSerializerOptions)}");
            Console.WriteLine("========================================");



            await _aiService.Ask<object>(JsonSerializer.Serialize(stockPrompt, _jsonSerializerOptions));
            //var analysis = await _aiService.Ask<StockAnalysis>("{\"AnalysisRequest\":\"Analyze the given stock data, financial statements, and market sentiment to determine whether now is a good time to BUY, SELL, or HOLD RCAT. Consider price trends, trading volume, earnings, and recent news sentiment in your response. Give me short response.\",\"InvestmentHorizon\":\"Short-Term (1-4 weeks)\"},\"Stock\":{\"Quote\":{\"Symbol\":\"RCAT\",\"Name\":\"Red Cat Holdings, Inc.\",\"Price\":4.865,\"ChangesPercentage\":-1.51822,\"Change\":-0.075,\"DayLow\":4.78,\"DayHigh\":5.32,\"YearHigh\":15.274,\"YearLow\":0.7,\"MarketCap\":416529624,\"PriceAvg50\":9.1017,\"PriceAvg200\":4.86016,\"Exchange\":\"NASDAQ\",\"Volume\":2181067,\"AvgVolume\":10121643,\"Open\":5.16,\"PreviousClose\":4.94,\"Eps\":-0.51,\"Pe\":-9.54,\"EarningsAnnouncement\":\"2025-03-17T14:59:00Z\",\"SharesOutstanding\":85617600,\"Timestamp\":\"2025-03-12T18:09:55Z\"},\"PrevDays\":[{\"Date\":\"2025-03-11\",\"Open\":\"4.9400\",\"High\":\"4.9900\",\"Low\":\"4.5800\",\"Close\":\"4.9400\",\"Volume\":\"2834826\"},{\"Date\":\"2025-03-10\",\"Open\":\"5.1600\",\"High\":\"5.2700\",\"Low\":\"4.6100\",\"Close\":\"4.8150\",\"Volume\":\"4942834\"},{\"Date\":\"2025-03-07\",\"Open\":\"5.2000\",\"High\":\"5.5590\",\"Low\":\"5.0500\",\"Close\":\"5.3900\",\"Volume\":\"3969176\"},{\"Date\":\"2025-03-06\",\"Open\":\"5.2700\",\"High\":\"5.5000\",\"Low\":\"5.0700\",\"Close\":\"5.2800\",\"Volume\":\"3682299\"},{\"Date\":\"2025-03-05\",\"Open\":\"5.4000\",\"High\":\"5.4299\",\"Low\":\"5.1600\",\"Close\":\"5.3900\",\"Volume\":\"5187666\"},{\"Date\":\"2025-03-04\",\"Open\":\"5.0700\",\"High\":\"5.5650\",\"Low\":\"5.0400\",\"Close\":\"5.4200\",\"Volume\":\"7872784\"},{\"Date\":\"2025-03-03\",\"Open\":\"6.3400\",\"High\":\"6.6261\",\"Low\":\"5.4600\",\"Close\":\"5.5700\",\"Volume\":\"6516820\"},{\"Date\":\"2025-02-28\",\"Open\":\"6.0650\",\"High\":\"6.3400\",\"Low\":\"5.9200\",\"Close\":\"6.2000\",\"Volume\":\"5746021\"},{\"Date\":\"2025-02-27\",\"Open\":\"6.8400\",\"High\":\"7.0300\",\"Low\":\"6.2800\",\"Close\":\"6.3000\",\"Volume\":\"6294422\"},{\"Date\":\"2025-02-26\",\"Open\":\"6.8000\",\"High\":\"7.1350\",\"Low\":\"6.6300\",\"Close\":\"6.7800\",\"Volume\":\"4492955\"}],\"AnnualReport\":{\"FiscalDateEnding\":\"2024-04-30\",\"ReportedCurrency\":\"USD\",\"TotalRevenue\":\"17836382\",\"CostOfRevenue\":\"14155836\",\"GrossProfit\":\"3680546\",\"OperatingIncome\":\"-21526696\",\"NetIncome\":\"-24052629\",\"DepreciationAndAmortization\":\"854311\"},\"News\":{\"Google\":[{\"Title\":\"Red Cat Announces Appointment Of Christian Koji Ericson As Chief Financial Officer -March 12, 2025 at 01:12 pm EDT\",\"Url\":\"https://www.marketscreener.com/quote/stock/RED-CAT-HOLDINGS-INC-120797502/news/Red-Cat-Announces-Appointment-Of-Christian-Koji-Ericson-As-Chief-Financial-Officer-49315741/\",\"Summary\":\"Red Cat Holdings Inc is a Puerto Rico-based provider of products, services and solutions to the drone industry. The Company provides its services to the...\",\"PublishTime\":\"2025-03-12T21:15:35Z\"},{\"Title\":\"Red Cat Announces Appointment of Christian Koji Ericson as Chief Financial Officer\",\"Url\":\"https://www.manilatimes.net/2025/03/12/tmt-newswire/globenewswire/red-cat-announces-appointment-of-christian-koji-ericson-as-chief-financial-officer/2072029\",\"Summary\":\"SALT LAKE CITY, March 12, 2025 (GLOBE NEWSWIRE) -- Red Cat Holdings, Inc. (Nasdaq: RCAT) (\\u0022Red Cat\\u0022 or the \\u0022Company\\u0022), a drone technology company...\",\"PublishTime\":\"2025-03-12T19:10:35Z\"}],\"Reddit\":[{\"Title\":\"Jeff may indeed have misled investors - Relooking at the Dec 2024 corporate update, other contract, past SRR contract with Skydio, average price of military drones.\",\"Url\":\"https://www.reddit.com/r/RCAT/comments/1j9b9ur/jeff_may_indeed_have_misled_investors_relooking/\",\"Body\":\"Reviewing the Dec 2024 financial results and corporate update (https://ir.redcatholdings.com/news-events/press-releases/detail/165/red-cat-holdings-reports-financial-results-for-fiscal-second-quarter-2025-and-provides-corporate-update), it does indee...\",\"Score\":10,\"CommentsCount\":20,\"CreateTime\":\"2025-03-12T04:43:47Z\"},{\"Title\":\"How you feeling on the earnings call?\",\"Url\":\"https://www.reddit.com/r/RCAT/comments/1j8p7g8/how_you_feeling_on_the_earnings_call/\",\"Body\":\"I\\u0027m hoping it bounces to 11 soon of course. That\\u0027s when I bought it at. But I\\u0027m looking at the financials and I\\u0027m hoping they have enough cash to whether this downturn.\",\"Score\":13,\"CommentsCount\":24,\"CreateTime\":\"2025-03-11T12:08:55Z\"}],\"Yahoo\":[{\"Title\":\"Highlighting Red Cat Holdings And Two Other Leading Growth Stocks With Insider Influence\",\"Url\":\"https://finance.yahoo.comN/A\",\"Summary\":\"In the current U.S. market landscape, stocks are showing signs of recovery following an encouraging Consumer Price Index report that has eased inflation concerns and sparked optimism for potential interest rate cuts. Amid this backdrop, growth companies with high insider ownership, like Red Cat Holdings and others, are gaining attention as investors seek stocks with strong internal confidence and potential resilience against market volatility.\",\"PublishTime\":\"Simply Wall St. \\u2022 1 hour ago\"},{\"Title\":\"Red Cat Announces Appointment of Christian Koji Ericson as Chief Financial Officer\",\"Url\":\"https://finance.yahoo.comN/A\",\"Summary\":\"SALT LAKE CITY, March 12, 2025 (GLOBE NEWSWIRE) -- Red Cat Holdings, Inc. (Nasdaq: RCAT) (\\u0022Red Cat\\u0022 or the \\u0022Company\\u0022), a drone technology company integrating robotic hardware and software for military, government, and commercial operations, today announced the appointment of Christian Koji Ericson as its new Chief Financial Officer (CFO), effective March 17, 2025. Ericson brings more than 20 years of finance and accounting experience, including 11 years with PricewaterhouseCoopers (PwC) and seni\",\"PublishTime\":\"GlobeNewswire \\u2022 3 hours ago\"}],\"Finnhub\":[{\"Category\":\"company\",\"Title\":\"Red Cat Announces Appointment Of Christian Koji Ericson As Chief Financial Officer\",\"Source\":\"Finnhub\",\"Summary\":\"Red Cat Holdings Inc: * RED CAT ANNOUNCES APPOINTMENT OF CHRISTIAN KOJI ERICSON ASCHIEFFINANCIAL OFFICERSource text:Further company coverage: ...\",\"Url\":\"https://finnhub.io/api/news?id=1302df72364fad29216bf0cbdad6a5fe760a7f225c5fb36f5d38e4b52d40a0c0\",\"CreateTime\":\"2025-03-12T13:12:28Z\"},{\"Category\":\"company\",\"Title\":\"RCAT Investor News: Rosen Law Firm Encourages Red Cat Holdings, Inc. Investors to Inquire About Securities Class Action Investigation - RCAT\",\"Source\":\"Finnhub\",\"Summary\":\"NEW YORK, March 5, 2025 /PRNewswire/ -- Why: Rosen Law Firm, a global investor...\",\"Url\":\"https://finnhub.io/api/news?id=c212ae0b5a57bfad8b64b33681631e559f07bc203ddd50a63dfbb193773ed9d6\",\"CreateTime\":\"2025-03-05T15:51:05Z\"}]},\"Symbol\":\"RCAT\",\"Name\":\"Red Cat Holdings Inc\",\"ReturnOnEquityTTM\":-1.0,\"PriceToSalesRatioTTM\":25.68,\"QuarterlyRevenueGrowthYOY\":-0.61}}");

            watch.Stop();
            Console.WriteLine("========================================");
            Console.WriteLine($"Execution Time: {watch.Elapsed.TotalSeconds}s");
            Console.WriteLine("========================================");
        }
    }
}
