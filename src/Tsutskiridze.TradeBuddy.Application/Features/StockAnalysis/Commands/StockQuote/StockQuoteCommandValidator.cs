using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis.Commands.StockQuote;

public sealed class StockQuoteCommandValidator : IValidator<StockQuoteCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(StockQuoteCommand obj, CancellationToken cancellationToken)
    {
        List<ValidationError> errors = [];

        if (obj.ChatId == 0)
        {
            errors.Add(new ValidationError(nameof(obj.ChatId), "ChatId is required"));
        }

        if (string.IsNullOrWhiteSpace(obj.Symbol) || obj.Symbol.Length >= 7)
        {
            errors.Add(new ValidationError(nameof(obj.Symbol), "Invalid symbol provided"));
        }

        return ValueTask.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors)
        );
    }
}
