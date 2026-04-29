namespace SharedKernel.Validations;

public class ValidationResult
{

    public bool IsValid { get; init; }

    public IReadOnlyList<ValidationError> Errors { get; init; } = Array.Empty<ValidationError>();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(params ValidationError[] errors)
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors));

        if (errors.Length == 0)
            throw new ArgumentException("At least one validation error must be provided.", nameof(errors));

        return new ValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }

    public static ValidationResult Failure(IEnumerable<ValidationError> errors)
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors));

        var errorArray = errors.ToArray();

        if (errorArray.Length == 0)
            throw new ArgumentException("At least one validation error must be provided.", nameof(errors));

        return new ValidationResult
        {
            IsValid = false,
            Errors = errorArray
        };
    }
}