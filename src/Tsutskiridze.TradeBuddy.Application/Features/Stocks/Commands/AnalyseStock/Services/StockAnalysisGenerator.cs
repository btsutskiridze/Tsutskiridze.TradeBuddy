using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tsutskiridze.TradeBuddy.Application.Abstractions.AI;
using Tsutskiridze.TradeBuddy.Application.DTOs.StockAnalysis;

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

    
    /*
     *todo:
     *Put analysis behind a command/query handler
     *and let failures surface as typed exceptions or a proper result object. 
     */
    public async Task<StockAnalysisOutcomeDto> AnalyzeAsync(string stock)
    {
        try
        {
            _logger.LogInformation("Generating AI stock analysis for {Stock}", stock);
            var analysis = await GenerateAsync(stock);

            return new StockAnalysisOutcomeDto
            {
                Symbol = stock,
                Report = analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stock analysis for {Stock}", stock);

            return new StockAnalysisOutcomeDto
            {
                Symbol = stock,
                Report = null
            };
        }
    }

    public async Task<StockAnalysisReportDto> GenerateAsync(string stock)
    {
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation("Fetching stock prompt for {Stock}", stock);
        var stockPrompt = await _promptBuilder.BuildAsync(stock);
        var stockPromptJson = JsonSerializer.Serialize(stockPrompt, JsonSerializerOpts);

        _logger.LogInformation("Sending stock prompt to AI service");
        var analysis = await _aiClient.Ask<StockAnalysisReportDto>(stockPromptJson);

        analysis.ExecutionTime = (DateTime.UtcNow - startedAt).TotalSeconds;
        _logger.LogInformation("StockAnalysis completed in {ElapsedSeconds}s", analysis.ExecutionTime);

        return analysis;
    }
}
