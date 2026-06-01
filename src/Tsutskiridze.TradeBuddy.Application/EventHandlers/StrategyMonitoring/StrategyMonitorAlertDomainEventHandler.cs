using SharedKernel.Data;
using SharedKernel.Events;
using Tsutskiridze.TradeBuddy.Application.Abstractions.Notifications;
using Tsutskiridze.TradeBuddy.Application.Features.Chats.Specifications;
using Tsutskiridze.TradeBuddy.Application.Features.StrategyMonitoring;
using Tsutskiridze.TradeBuddy.Application.Features.TradeStrategies.Specifications;
using Tsutskiridze.TradeBuddy.Application.Notifications;
using Tsutskiridze.TradeBuddy.Domain.Chats;
using Tsutskiridze.TradeBuddy.Domain.StrategyMonitoring.Events;
using Tsutskiridze.TradeBuddy.Domain.TradeStrategies;

namespace Tsutskiridze.TradeBuddy.Application.EventHandlers.StrategyMonitoring;

public sealed class StrategyMonitorAlertDomainEventHandler : IDomainEventHandler<StrategyMonitorAlertDomainEvent>
{
    private readonly IReadRepository<Chat> _chats;
    private readonly IReadRepository<TradeStrategy, int> _strategies;
    private readonly INotificationDispatcher _notifier;

    public StrategyMonitorAlertDomainEventHandler(
        IReadRepository<Chat> chats,
        IReadRepository<TradeStrategy, int> strategies,
        INotificationDispatcher notifier)
    {
        _chats = chats;
        _strategies = strategies;
        _notifier = notifier;
    }

    public async ValueTask Handle(StrategyMonitorAlertDomainEvent domainEvent, CancellationToken ct)
    {
        var chat = await _chats.FirstOrDefaultAsync(
            new ActiveChatTelegramIdsByIdsSpec([domainEvent.ChatId]),
            ct);

        if (chat is null)
            return;

        var strategy = await _strategies.FirstOrDefaultAsync(
            new TradeStrategyCodeByIdSpec(domainEvent.TradeStrategyId),
            ct);

        if (strategy is null)
            return;

        await _notifier.DispatchAsync(
            new StrategyMonitorAlertNotification(
                StrategyMonitorEvaluationSummary.Create(
                    chat.TelegramId,
                    strategy.ToString(),
                    domainEvent.Symbol,
                    domainEvent.Evaluation)
            ),
            ct
        );
    }
}