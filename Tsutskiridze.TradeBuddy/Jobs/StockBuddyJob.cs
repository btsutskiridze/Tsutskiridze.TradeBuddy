using Newtonsoft.Json;
using System.Diagnostics;
using Tsutskiridze.Bloom.Core.Infrastructure.Jobs;
using Tsutskiridze.TradeBuddy.Helpers;
using Tsutskiridze.TradeBuddy.Models;
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

        private readonly StockPromptService _stockPromptService;
        private readonly IAIService _aiService;
        private readonly ILogger<StockBuddyJob> _logger;

        private static JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public StockBuddyJob(StockPromptService stockPromptService, IAIService geminiService, ILogger<StockBuddyJob> logger)
        {
            _stockPromptService = stockPromptService;
            _aiService = geminiService;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();
            var stock = Stocks.MVST;

            try
            {
                _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
                var stockPrompt = await _stockPromptService.GetStockPrompt(stock);
                var stockPromptJson = JsonConvert.SerializeObject(stockPrompt, _jsonSettings);

                _logger.LogInformation("Sending stock prompt to AI service");
                await _aiService.Ask<StockAnalysisModels.StockAnalysis>(stockPromptJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock prompt for {Stock}", stock);
            }

            watch.Stop();
            _logger.LogInformation("{Name} completed in {ElapsedSeconds}s", Name, watch.Elapsed.TotalSeconds);
        }

        public string CreateTelegramMessage(StockAnalysisModels.StockAnalysis analysis)
        {
            // Build the message using string interpolation
            var message = $"🚨 Stock Alert: PLTR 🚨\n" +
                          $"- 📈 Current Price: {analysis.price}\n" +
                          $"- 📊 50-day Avg: {analysis.bench.avg50} | Year High: {analysis.bench.yearHigh}\n" +
                          $"- 🔔 Trading Volume: {analysis.volAnalysis}\n" +
                          $"- 📰 Overall News: {analysis.newsOverall.conf} Positive\n" +
                          $"- 🤖 AI Analysis: {analysis.ai.rec} ({analysis.ai.conf} Confidence)\n" +
                          $"- ⏱️ Next Update: 15 mins";

            return message;
        }

    }
}
