namespace SharedKernel.Validations;

public interface IValidator<in T>
{
    ValueTask<ValidationResult> ValidateAsync(T obj, CancellationToken cancellationToken);
}