namespace SharedKernel.Events;

public interface IDomainEventHandler<in T> : IBaseNotificationHandler<T> where T : IDomainEvent
{
}