using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Finnhub;

public class FinnhubNewsQueryValidator : IValidator<FinnhubNewsQuery>
{
    public ValueTask<ValidationResult> ValidateAsync(FinnhubNewsQuery obj, CancellationToken cancellationToken)
    {
        if (obj.Limit is <= 0 or > 10)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(
                    nameof(obj.Limit),
                    "Limit must be between 1 and 10"
                )
            ]));
        }

        if (string.IsNullOrWhiteSpace(obj.Symbol) || obj.Symbol.Length >= 7)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(
                    nameof(obj.Symbol),
                    "Invalid Symbol provided"
                )
            ]));
        }

        if (obj is { From: not null, To: not null } && obj.From > obj.To)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(
                    nameof(obj.From),
                    "From date must be less than or equal to To date"
                )
            ]));
        }

        return ValueTask.FromResult(ValidationResult.Success());
    }
}
