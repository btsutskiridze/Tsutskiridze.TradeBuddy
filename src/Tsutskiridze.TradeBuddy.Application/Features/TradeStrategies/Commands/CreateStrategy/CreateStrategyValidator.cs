using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.CreateStrategy;

public sealed class CreateStrategyValidator : IValidator<CreateStrategyCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(CreateStrategyCommand obj, CancellationToken cancellationToken)
    {
        List<ValidationError> errors = [];

        if (obj.ChatId == 0)
        {
            errors.Add(new ValidationError(nameof(obj.ChatId), "ChatId is required"));
        }

        if (!Enum.IsDefined(obj.Timeframe))
        {
            errors.Add(new ValidationError(nameof(obj.Timeframe), "Invalid timeframe provided"));
        }

        if (obj.EmaFastPeriod <= 0)
        {
            errors.Add(new ValidationError(nameof(obj.EmaFastPeriod), "EMA fast period must be positive"));
        }

        if (obj.EmaSlowPeriod <= 0)
        {
            errors.Add(new ValidationError(nameof(obj.EmaSlowPeriod), "EMA slow period must be positive"));
        }

        if (obj.EmaSlowPeriod <= obj.EmaFastPeriod)
        {
            errors.Add(new ValidationError(
                nameof(obj.EmaSlowPeriod),
                "EMA slow period must be greater than EMA fast period"));
        }

        if (obj.AdxPeriod <= 0)
        {
            errors.Add(new ValidationError(nameof(obj.AdxPeriod), "ADX period must be positive"));
        }

        if (obj.AdxTrendStrengthThreshold <= 0)
        {
            errors.Add(new ValidationError(
                nameof(obj.AdxTrendStrengthThreshold),
                "ADX trend strength threshold must be positive"));
        }

        if (obj.AdxNonFallingLookBackBars <= 0)
        {
            errors.Add(new ValidationError(
                nameof(obj.AdxNonFallingLookBackBars),
                "ADX non-falling lookback bars must be positive"));
        }

        if (obj.AtrPeriod <= 0)
        {
            errors.Add(new ValidationError(nameof(obj.AtrPeriod), "ATR period must be positive"));
        }

        if (obj.AtrInitialStopMultiplier <= 0)
        {
            errors.Add(new ValidationError(
                nameof(obj.AtrInitialStopMultiplier),
                "ATR initial stop multiplier must be positive"));
        }

        if (obj.AtrTrailingStopMultiplier <= 0)
        {
            errors.Add(new ValidationError(
                nameof(obj.AtrTrailingStopMultiplier),
                "ATR trailing stop multiplier must be positive"));
        }

        if (obj.AtrTrailingActivationMultiplier <= 0)
        {
            errors.Add(new ValidationError(
                nameof(obj.AtrTrailingActivationMultiplier),
                "ATR trailing activation multiplier must be positive"));
        }

        return ValueTask.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors)
        );
    }
}
