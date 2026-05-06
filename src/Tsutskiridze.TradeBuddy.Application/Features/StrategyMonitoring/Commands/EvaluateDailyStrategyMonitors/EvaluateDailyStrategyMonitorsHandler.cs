using Mediator;

namespace Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.EvaluateDailyStrategyMonitors;


public sealed record EvaluateDailyStrategyMonitorsCommand(DateOnly TradingDate) : ICommand;

public class EvaluateDailyStrategyMonitorsHandler:ICommandHandler<EvaluateDailyStrategyMonitorsCommand>
{
    public async ValueTask<Unit> Handle(EvaluateDailyStrategyMonitorsCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}