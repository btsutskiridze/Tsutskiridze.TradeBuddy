using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.DeleteStrategy;

public sealed class DeleteStrategyValidator : IValidator<DeleteStrategyCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(DeleteStrategyCommand obj, CancellationToken cancellationToken)
    {
        List<ValidationError> errors = [];

        if (obj.ChatId == 0)
        {
            errors.Add(new ValidationError(nameof(obj.ChatId), "ChatId is required"));
        }

        if (string.IsNullOrWhiteSpace(obj.StrategyCode))
        {
            errors.Add(new ValidationError(nameof(obj.StrategyCode), "StrategyCode is required"));
        }

        return ValueTask.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors)
        );
    }
}
