namespace SharedKernel.Validations;

public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }
    
    public ValidationException(IReadOnlyList<ValidationError> errors)
        : base($"Validation failed with {errors?.Count ?? 0} error(s)")
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors), "Validation errors cannot be null");

        if (errors.Count == 0)
            throw new ArgumentException("At least one validation error must be provided", nameof(errors));

        Errors = errors;
    }

    public ValidationException(params ValidationError[] errors)
        : this((IReadOnlyList<ValidationError>)errors)
    {
    }

    public ValidationException(string message, IReadOnlyList<ValidationError> errors)
        : base(message)
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors), "Validation errors cannot be null");

        if (errors.Count == 0)
            throw new ArgumentException("At least one validation error must be provided", nameof(errors));

        Errors = errors;
    }
    
    public ValidationException(string message, Exception innerException, IReadOnlyList<ValidationError> errors)
        : base(message, innerException)
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors), "Validation errors cannot be null");

        if (errors.Count == 0)
            throw new ArgumentException("At least one validation error must be provided", nameof(errors));

        Errors = errors;
    }
}