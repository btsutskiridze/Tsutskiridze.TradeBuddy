using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring.Commands.ActivateStrategyMonitor;
using Tsutskiridze.TradeBuddy.Infrastructure.TextFormatting.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public sealed class ActivateStrategyMonitorTelegramCommandHandler : ITelegramCommandHandler
{
    private static string Usage => $"Usage:\n{TelegramCommandCatalog.MonitorStrategy.Usage}";

    private readonly IMediator _mediator;

    public ActivateStrategyMonitorTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.MonitorStrategy.Command;
    public string Description => TelegramCommandCatalog.MonitorStrategy.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(
        TelegramCommandDispatchRequest dispatchRequest,
        CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 2)
        {
            throw new TelegramPresentationException(Usage);
        }

        var symbol = dispatchRequest.Args[0];
        var strategyCode = dispatchRequest.Args[1];

        if (string.IsNullOrWhiteSpace(strategyCode) || string.IsNullOrWhiteSpace(symbol))
        {
            throw new TelegramPresentationException(Usage);
        }

        var result = await _mediator.Send(
            new ActivateStrategyMonitorCommand(dispatchRequest.ChatId, strategyCode, symbol),
            ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId,
            StrategyMonitorMessageFormatter.CreateActivationMessage(result),
            ParseMode.Markdown);
    }
}