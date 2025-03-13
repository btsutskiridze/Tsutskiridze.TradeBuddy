using Newtonsoft.Json;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Tsutskiridze.TradeBuddy.DTOs;
using Tsutskiridze.TradeBuddy.Jobs;
using Tsutskiridze.TradeBuddy.Models;
using Tsutskiridze.TradeBuddy.Services.AI;

namespace Tsutskiridze.TradeBuddy.Services
{
    public class StockAnalysisService
    {
        private readonly StockPromptService _stockPromptService;
        private readonly IAIService _aiService;
        private readonly TelegramService _telegramService;
        private readonly ILogger<StockBuddyJob> _logger;

        private static JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public StockAnalysisService(StockPromptService stockPromptService, IAIService geminiService, TelegramService telegramService, ILogger<StockBuddyJob> logger)
        {
            _stockPromptService = stockPromptService;
            _aiService = geminiService;
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task<List<ExecuteStockAnalysisResponse>> ExecuteStockAnalysisMultiple(List<string> stocks)
        {
            _logger.LogInformation("Starting stock analysis for {Stocks}", string.Join(", ", stocks));
            var tasks = stocks.Select(ExecuteStockAnalysis).ToList();

            var results = await Task.WhenAll(tasks);
            _logger.LogInformation("All stock analysis completed");

            return results.OfType<ExecuteStockAnalysisResponse>().ToList();
        }

        public async Task<ExecuteStockAnalysisResponse?> ExecuteStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Generating AI stock analysis for {Stock}", stock);
                var analysis = await GenerateAiStockAnalysis(stock);

                if (analysis == null)
                {
                    _logger.LogWarning("StockAnalysis returned null for {Stock}", stock);
                    return null;
                }

                _logger.LogInformation("Sending stock analysis to Telegram");
                await SendTelegramMessage(analysis);

                return new ExecuteStockAnalysisResponse
                {
                    Analysis = analysis,
                    TelegramMessage = CreateStockTelegramMessage(analysis)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock analysis for {Stock}", stock);
                return null;
            }
            finally
            {
                watch.Stop();
            }
        }

        private async Task<StockAnalysisModels.StockAnalysis?> GenerateAiStockAnalysis(string stock)
        {
            var watch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
                var stockPrompt = await _stockPromptService.GetStockPrompt(stock);
                var stockPromptJson = JsonConvert.SerializeObject(stockPrompt, _jsonSettings);

                if (stockPrompt == null)
                {
                    _logger.LogWarning("StockPrompt returned null for {Stock}", stock);
                    return null;
                }

                _logger.LogInformation("Sending stock prompt to AI service");
                var analysis = await _aiService.Ask<StockAnalysisModels.StockAnalysis>(stockPromptJson);

                if (analysis == null)
                {
                    _logger.LogWarning("StockAnalysis returned null for {Stock}", stock);
                    return null;
                }

                analysis.ExecutionTime = watch.Elapsed.TotalSeconds;
                _logger.LogInformation("StockAnalysis completed in {ElapsedSeconds}s", analysis.ExecutionTime);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock prompt for {Stock}", stock);
                return null;
            }
            finally
            {
                watch.Stop();
            }
        }

        private async Task SendTelegramMessage(StockAnalysisModels.StockAnalysis analysis)
        {
            var message = CreateStockTelegramMessage(analysis);
            await _telegramService.SendMessage(message);
        }

        private string CreateStockTelegramMessage(StockAnalysisModels.StockAnalysis analysis)
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

        public async Task ListenTelegramChat()
        {
            var botClient = _telegramService.GetBotClient();

            using var cts = new CancellationTokenSource();

            // Start receiving updates
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                cancellationToken: cts.Token,
                receiverOptions: new Telegram.Bot.Polling.ReceiverOptions
                {
                    DropPendingUpdates = true,
                });

            Console.WriteLine("Listening for updates. Press any key to exit.");
            Console.ReadKey();

            // Cancel receiving updates when you're ready to stop
            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            if (update.Message != null)
            {
                Console.WriteLine($"Received a message in chat ID: {update.Message.Chat.Id}");
                Console.WriteLine($"Message text: {update.Message.Text}");
                if (update.Message.Text.ToLower().StartsWith("stock:"))
                {
                    var stock = update.Message.Text.Split(":")[1].Trim();

                    await botClient.SendMessage(update.Message.Chat.Id, $"Analyzing stock: {stock}", cancellationToken: cancellationToken);

                    var response = await ExecuteStockAnalysis(stock);

                    if (response == null)
                    {
                        await botClient.SendMessage(update.Message.Chat.Id, $"Failed analyzing stock: {stock}", cancellationToken: cancellationToken);
                    }
                }
            }
            await Task.CompletedTask;
        }

        // Handles any errors that occur during update processing
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            string errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
