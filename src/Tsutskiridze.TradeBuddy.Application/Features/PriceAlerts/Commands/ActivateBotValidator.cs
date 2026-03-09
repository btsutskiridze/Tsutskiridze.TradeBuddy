using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands;

public class ActivateBotValidator : IValidator<ActivateBotCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(ActivateBotCommand obj, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(obj.Token))
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(nameof(obj.Token), "Token Required")
            ]));
        }
        
        return ValueTask.FromResult(ValidationResult.Success());
    }
}