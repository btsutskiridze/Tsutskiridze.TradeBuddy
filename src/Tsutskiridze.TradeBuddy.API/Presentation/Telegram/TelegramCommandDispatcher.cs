using System.Text;
using SharedKernel;
using SharedKernel.Validations;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Abstractions;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Commands;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Contracts;
using Tsutskiridze.TradeBuddy.API.Presentation.Telegram.Errors;
using Tsutskiridze.TradeBuddy.Infrastructure.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.API.Presentation.Telegram;

public class TelegramCommandDispatcher : ITelegramCommandDispatcher
{
    private readonly IReadOnlyDictionary<string, ITelegramCommandHandler> _handlers;
    private readonly ITelegramErrorResponseFactory _errorFactory;
    private readonly ILogger<TelegramCommandDispatcher> _logger;
    private readonly ITelegramSender _sender;

    public TelegramCommandDispatcher(
        IEnumerable<ITelegramCommandHandler> handlers,
        ITelegramErrorResponseFactory errorFactory,
        ILogger<TelegramCommandDispatcher> logger, ITelegramSender sender)
    {
        _handlers = handlers.ToDictionary(x => x.Command, StringComparer.OrdinalIgnoreCase);
        _errorFactory = errorFactory;
        _logger = logger;
        _sender = sender;
    }

    public async Task<TelegramCommandDispatchResult> Dispatch(TelegramCommandRequest request, CancellationToken ct)
    {
        if (!_handlers.TryGetValue(request.Command, out var handler))
        {
            return TelegramCommandDispatchResult.TextReply(
                request.ChatId,
                $"Unknown command: `{request.Command}`. Try `/help`.",
                ParseMode.Markdown
            );
        }

        try
        {
            var result = await handler.Handle(request, ct);
            
            if (result.HasPriorMessages)
                await _sender.Send(result.PriorMessages, ct);
            
            return result;
        }
        catch (Exception ex)
        {
            return _errorFactory.Create(request.ChatId, ex);
        }
    }
}