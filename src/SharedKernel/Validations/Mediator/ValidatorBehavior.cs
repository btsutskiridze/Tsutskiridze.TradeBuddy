using Mediator;

namespace SharedKernel.Validations.Mediator;

public sealed class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidatorBehavior(
        IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(request, cancellationToken);
        }

        var errors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);

            if (result.IsValid) continue;

            errors.AddRange(result.Errors);
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors.AsReadOnly());
        }

        return await next(request, cancellationToken);
    }
}