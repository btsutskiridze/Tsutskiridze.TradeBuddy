using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands.MyAlerts;

public sealed class MyAlertsValidator : IValidator<MyAlertsCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(MyAlertsCommand obj, CancellationToken cancellationToken)
    {
        if (obj.ChatId == 0)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(nameof(obj.ChatId), "ChatId is required")
            ]));
        }

        return ValueTask.FromResult(ValidationResult.Success());
    }
}
