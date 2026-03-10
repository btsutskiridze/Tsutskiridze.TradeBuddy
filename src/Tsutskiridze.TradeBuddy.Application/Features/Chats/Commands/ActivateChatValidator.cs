using SharedKernel.Validations;

namespace Tsutskiridze.TradeBuddy.Application.Features.Chats.Commands;

public class ActivateChatValidator : IValidator<ActivateBotCommand>
{
    public ValueTask<ValidationResult> ValidateAsync(ActivateBotCommand obj, CancellationToken cancellationToken)
    {
        if (obj.ChatId == 0)
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(nameof(obj.ChatId), "chatId Required")
            ]));
        }
        
        if (string.IsNullOrEmpty(obj.Token))
        {
            return ValueTask.FromResult(ValidationResult.Failure([
                new ValidationError(nameof(obj.Token), "Token Required")
            ]));
        }
        
        return ValueTask.FromResult(ValidationResult.Success());
    }
}