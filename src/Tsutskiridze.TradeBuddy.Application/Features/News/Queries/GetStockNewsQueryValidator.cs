using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.News.Queries;

public sealed class GetStockNewsQueryValidator : IValidator<GetStockNewsQuery>
{
    public ValueTask<ValidationResult> ValidateAsync(GetStockNewsQuery obj, CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        if (obj.Limit is <= 0 or > 10)
        {
            errors.Add(new ValidationError(
                nameof(obj.Limit),
                "Limit must be between 1 and 10"
            ));
        }

        if (string.IsNullOrWhiteSpace(obj.Symbol) || obj.Symbol.Length >= 7)
        {
            errors.Add(new ValidationError(
                nameof(obj.Symbol),
                "Invalid Symbol provided"
            ));
        }

        if (obj is { From: not null, To: not null } && obj.From > obj.To)
        {
            errors.Add(new ValidationError(
                nameof(obj.From),
                "From date must be less than or equal to To date"
            ));
        }

        if (!Enum.IsDefined(obj.SortType))
        {
            errors.Add(new ValidationError(
                nameof(obj.SortType),
                "Invalid SortType provided"
            ));
        }

        if (errors.Count > 0)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                .. errors
            ]));
        }

        return ValueTask.FromResult(ValidationResult.Success());
    }
}