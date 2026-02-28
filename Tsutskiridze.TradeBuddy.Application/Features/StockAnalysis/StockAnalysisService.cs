using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Dtos;
using Tsutskiridze.TradeBuddy.Application.Interfaces.AI;
using Tsutskiridze.TradeBuddy.Application.Options;
using Tsutskiridze.TradeBuddy.Core.Entities;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis
{
    //TODO: refactor
    public class StockAnalysisService
    {
        private readonly StockPromptService _stockPromptService;
        private readonly IAIService _aiService;
        private readonly ITelegramBotClient _telegramBot;
        private readonly ILogger<StockAnalysisService> _logger;
        private readonly TelegramBotOptions _options;

        private static JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public StockAnalysisService(
            StockPromptService stockPromptService,
            IAIService aiService,
            ITelegramBotClient telegramBot,
            ILogger<StockAnalysisService> logger,
            IOptions<TelegramBotOptions> options
        )
        {
            _stockPromptService = stockPromptService;
            _aiService = aiService;
            _telegramBot = telegramBot;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<List<ExecuteStockAnalysisResponse>> ExecuteStockAnalysisMultiple(List<string> stocks)
        {
            _logger.LogInformation("Starting stock analysis for {Stocks}", string.Join(", ", stocks));
            var tasks = stocks.Select(ExecuteStockAnalysis).ToList();

            var results = await Task.WhenAll(tasks);
            _logger.LogInformation("All stock analysis completed");

            return [.. results];
        }

        public async Task<List<ExecuteStockAnalysisResponse>> ExecuteStockAnalysisMultiple(List<string> stocks, TimeSpan delay)
        {
            _logger.LogInformation("Starting stock analysis for {Stocks}", string.Join(", ", stocks));
            var results = new List<ExecuteStockAnalysisResponse>();

            var lastStock = stocks.Last();

            foreach (var stock in stocks)
            {
                var result = await ExecuteStockAnalysis(stock);
                results.Add(result);

                if (stock != lastStock)
                {
                    _logger.LogInformation("Waiting for {Delay} before next analysis", delay);
                    await Task.Delay(delay);
                }
            }

            _logger.LogInformation("All stock analysis completed");
            return results;
        }

        public async Task<ExecuteStockAnalysisResponse> ExecuteStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating AI stock analysis for {Stock}", stock);
                var analysis = await GenerateAiStockAnalysis(stock);

                _logger.LogInformation("Sending stock analysis to Telegram");
                await SendTelegramMessage(analysis);

                return new ExecuteStockAnalysisResponse
                {
                    Stock = stock,
                    Analysis = analysis,
                    TelegramMessage = CreateStockTelegramMessage(analysis)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock analysis for {Stock}", stock);

                return new ExecuteStockAnalysisResponse
                {
                    Stock = stock,
                    Analysis = null,
                    TelegramMessage = $"Failed analyzing stock: {stock}"
                };
            }
            finally
            {
                watch.Stop();
            }
        }

        public async Task<StockAnalysisModels.StockAnalysis> GenerateAiStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
                var stockPrompt = await _stockPromptService.GetStockPrompt(stock);
                var stockPromptJson = JsonConvert.SerializeObject(stockPrompt, _jsonSettings);

                _logger.LogInformation("Sending stock prompt to AI service");
                var analysis = await _aiService.Ask<StockAnalysisModels.StockAnalysis>(stockPromptJson);

                analysis.ExecutionTime = watch.Elapsed.TotalSeconds;
                _logger.LogInformation("StockAnalysis completed in {ElapsedSeconds}s", analysis.ExecutionTime);

                return analysis;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                watch.Stop();
            }
        }

        private async Task SendTelegramMessage(StockAnalysisModels.StockAnalysis analysis)
        {
            var message = CreateStockTelegramMessage(analysis);
            await _telegramBot.SendMessage(_options.GroupChatID, message);
        }

        public async Task SendStockAnalysisToTelegram(long chatID, StockAnalysisModels.StockAnalysis analysis)
        {
            try
            {
                var message = CreateStockTelegramMessage(analysis);
                await _telegramBot.SendMessage(chatID, message);
                _logger.LogInformation("Stock analysis sent to Telegram");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending stock analysis to Telegram");
                await _telegramBot.SendMessage(chatID, "Failed to send stock analysis");
            }
        }

        public static string CreateStockTelegramMessage(StockAnalysisModels.StockAnalysis analysis)
        {
            // Build the message using string interpolation
            var message = $"🚨 Stock Alert: {analysis.Symbol} 🚨\n" +
                          $"- 📈 Current Price: {analysis.price}\n" +
                          $"- 📊 50-day Avg: {analysis.bench.avg50} | Year High: {analysis.bench.yearHigh}\n" +
                          $"- 🔔 Trading Volume: {analysis.volAnalysis}\n" +
                          $"- 📰 Overall News: {analysis.newsOverall.conf} Positive\n" +
                          $"- 🤖 AI Analysis: {analysis.ai.rec} ({analysis.ai.conf} Confidence)\n" +
                          $"- ⏱️ Analysis Duration: {string.Format("{0:0.00}", analysis.ExecutionTime)}s";

            return message;
        }
    }
}
