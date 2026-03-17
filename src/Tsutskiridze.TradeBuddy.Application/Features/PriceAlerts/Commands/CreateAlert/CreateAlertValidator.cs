using SharedKernel.Validations;
using Tsutskiridze.TradeBuddy.Core.Enums;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.CreateAlert;

public class CreateAlertValidator : IValidator<CreateAlertCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(CreateAlertCommand obj, CancellationToken cancellationToken)
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

        if (!Enum.IsDefined(obj.Direction))
        {
            errors.Add(new ValidationError(nameof(obj.Direction), "Invalid direction provided"));
        }

        if (obj.Price < 0)
        {
            errors.Add(new ValidationError(nameof(obj.Price), "Price must be greater than or equal to 0"));
        }

        return ValueTask.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors)
        );
    }
}