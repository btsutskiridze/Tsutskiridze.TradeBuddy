using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries.Reddit;

public class RedditNewsQueryValidator : IValidator<RedditNewsQuery>
{
    public ValueTask<ValidationResult> ValidateAsync(RedditNewsQuery obj, CancellationToken cancellationToken)
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

        return ValueTask.FromResult(ValidationResult.Success());
    }
}
