using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Queries.GetMyAlerts;

public sealed class GetMyAlertsQueryValidator : IValidator<MyAlertsQuery>
{
    public ValueTask<ValidationResult> ValidateAsync(MyAlertsQuery obj, CancellationToken cancellationToken)
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
