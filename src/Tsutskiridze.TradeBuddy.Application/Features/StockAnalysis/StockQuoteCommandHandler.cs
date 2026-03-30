using Mediator;
using Tsutskiridze.TradeBuddy.Application.DTOs.Notifications.Telegram;

namespace Tsutskiridze.TradeBuddy.Application.Features.StockAnalysis;

public record StockQuoteCommand(long ChatId, string Symbol) : ICommand<TelegramUpdateResultDto>;

public class StockQuoteCommandHandler : ICommandHandler<StockQuoteCommand, TelegramUpdateResultDto>
{
    public async ValueTask<TelegramUpdateResultDto> Handle(StockQuoteCommand command, CancellationToken cancellationToken)
    {
    }
}