using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

public sealed class ActivateChatValidator : IValidator<ActivateChatCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(ActivateChatCommand obj, CancellationToken cancellationToken)
    {
        List<ValidationError> errors = [];

        if (obj.ChatId == 0)
        {
            errors.Add(new ValidationError(nameof(obj.ChatId), "chatId Required"));
        }
        
        if (string.IsNullOrWhiteSpace(obj.Token))
        {
            errors.Add(new ValidationError(nameof(obj.Token), "Token Required"));
        }
        
        return ValueTask.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors)
        );
    }
}