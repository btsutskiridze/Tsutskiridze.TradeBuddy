using Mediator;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Commands.DeleteStrategy;

namespace Tsutskiridze.TradeBuddy.API.Telegram.Commands;

public sealed class DeleteStrategyTelegramCommandHandler : ITelegramCommandHandler
{
    private static string Usage => $"Usage:\n{TelegramCommandCatalog.DeleteStrategy.Usage}";

    private readonly IMediator _mediator;

    public DeleteStrategyTelegramCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Command => TelegramCommandCatalog.DeleteStrategy.Command;
    public string Description => TelegramCommandCatalog.DeleteStrategy.Description;

    public async Task<TelegramCommandDispatchResponse> Handle(
        TelegramCommandDispatchRequest dispatchRequest,
        CancellationToken ct)
    {
        if (dispatchRequest.Args.Count != 1)
        {
            throw new TelegramPresentationException(Usage);
        }

        var result = await _mediator.Send(
            new DeleteStrategyCommand(dispatchRequest.ChatId, dispatchRequest.Args[0]),
            ct);

        return TelegramCommandDispatchResponse.TextReply(
            dispatchRequest.ChatId,
            $"Trade strategy `{result.StrategyCode}` has been deleted.",
            ParseMode.Markdown);
    }
}
