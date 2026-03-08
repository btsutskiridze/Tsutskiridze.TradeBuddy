namespace SharedKernel.Validations;

public interface IValidator<in T>
{
    ValueTask<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken);
}