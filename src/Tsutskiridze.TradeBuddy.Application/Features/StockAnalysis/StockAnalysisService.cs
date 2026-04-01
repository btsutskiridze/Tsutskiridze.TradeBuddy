using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using Telegram.Bot;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;
using Tsutskiridze.TradeBuddy.Application.Options;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis
{
    //TODO: refactor
    public class StockAnalysisService
    {
        private readonly StockPromptService _stockPromptService;
        private readonly IAiService _aiService;
        private readonly ITelegramBotClient _telegramBot;
        private readonly ILogger<StockAnalysisService> _logger;
        private readonly TelegramBotOptions _options;

        private static JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public StockAnalysisService(
            StockPromptService stockPromptService,
            IAiService aiService,
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

        public async Task<List<StockAnalysisExecutionResultDto>> ExecuteStockAnalysisMultiple(List<string> stocks)
        {
            _logger.LogInformation("Starting stock analysis for {Stocks}", string.Join(", ", stocks));
            var tasks = stocks.Select(ExecuteStockAnalysis).ToList();

            var results = await Task.WhenAll(tasks);
            _logger.LogInformation("All stock analysis completed");

            return [.. results];
        }

        public async Task<List<StockAnalysisExecutionResultDto>> ExecuteStockAnalysisMultiple(List<string> stocks, TimeSpan delay)
        {
            _logger.LogInformation("Starting stock analysis for {Stocks}", string.Join(", ", stocks));
            var results = new List<StockAnalysisExecutionResultDto>();

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

        public async Task<StockAnalysisExecutionResultDto> ExecuteStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating AI stock analysis for {Stock}", stock);
                var analysis = await GenerateAiStockAnalysis(stock);

                _logger.LogInformation("Sending stock analysis to Telegram");

                return new StockAnalysisExecutionResultDto
                {
                    Stock = stock,
                    Analysis = analysis
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock analysis for {Stock}", stock);

                return new StockAnalysisExecutionResultDto
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

        public async Task<StockAnalysisResultDto> GenerateAiStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
                var stockPrompt = await _stockPromptService.GetStockPrompt(stock);
                var stockPromptJson = JsonConvert.SerializeObject(stockPrompt, _jsonSettings);

                _logger.LogInformation("Sending stock prompt to AI service");
                var analysis = await _aiService.Ask<StockAnalysisResultDto>(stockPromptJson);

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
    }
}
