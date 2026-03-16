namespace SharedKernel.Validations;

public class ValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(string error)
        : base($"Validation failed with error")
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentNullException(nameof(error), "Error message cannot be null");

        Errors = [new ValidationError("Error", error)];
    }

    public ValidationException(string prop, string error)
        : base($"Validation failed with error")
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentNullException(nameof(error), "Error message cannot be null");

        if (string.IsNullOrEmpty(prop))
            throw new ArgumentNullException(nameof(prop), "Prop message cannot be null");

        Errors = [new ValidationError(prop, error)];
    }

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