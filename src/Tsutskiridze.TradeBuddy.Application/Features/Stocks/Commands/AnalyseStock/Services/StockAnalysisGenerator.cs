using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Application.DTOs.Stocks;

namespace Tsutskiridze.TradeBuddy.Application.Features.Stocks.Commands.AnalyseStock.Services;

public class StockAnalysisGenerator
{
    private static readonly JsonSerializerOptions JsonSerializerOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly StockAnalysisPromptBuilder _promptBuilder;
    private readonly IAiClient _aiClient;
    private readonly ILogger<StockAnalysisGenerator> _logger;

    public StockAnalysisGenerator(
        StockAnalysisPromptBuilder promptBuilder,
        IAiClient aiClient,
        ILogger<StockAnalysisGenerator> logger)
    {
        _promptBuilder = promptBuilder;
        _aiClient = aiClient;
        _logger = logger;
    }

    public async Task<AnalyseStockResult> GenerateAsync(string stock)
    {
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
        var stockPrompt = await _promptBuilder.BuildAsync(stock);
        var stockPromptJson = JsonSerializer.Serialize(stockPrompt, JsonSerializerOpts);

        _logger.LogInformation("Sending stock prompt to AI service");
        var analysis = await _aiClient.Ask<AnalyseStockResult>(stockPromptJson);

        analysis.ExecutionTime = (DateTime.UtcNow - startedAt).TotalSeconds;
        _logger.LogInformation("StockAnalysis completed in {ElapsedSeconds}s", analysis.ExecutionTime);

        return analysis;
    }
}
