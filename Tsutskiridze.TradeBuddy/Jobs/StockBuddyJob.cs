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
        private readonly GeminiService _geminiService;
        public StockBuddyJob(StockPromptService stockPromptService, IOptions<JsonSerializerOptions> jsonSerializerOptions, GeminiService geminiService)
        {
            _stockPromptService = stockPromptService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
            _geminiService = geminiService;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var watch = Stopwatch.StartNew();

            var stockPrompt = await _stockPromptService.GetStockPrompt(Stocks.RCAT);

            await _geminiService.Ask(JsonSerializer.Serialize(stockPrompt, _jsonSerializerOptions));

            watch.Stop();
            Console.WriteLine($"Execution Time: {watch.Elapsed.TotalSeconds} s");
        }

    }
}
