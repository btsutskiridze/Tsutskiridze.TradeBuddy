using Mediator;

namespace Tsutskiridze.TradeBuddy.Application.Features.PriceAlerts.Commands;

public record ActivateBotCommand(string Token) : ICommand;

public sealed class ActivateBotHandler : ICommandHandler<ActivateBotCommand>
{
    public async ValueTask<Unit> Handle(ActivateBotCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}