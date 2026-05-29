namespace SharedKernel.Events;

public interface IDomainEvent : IBaseNotification
{
    public Guid Id { get; }
    public string EventType { get; }
    public DateTime CreateTime { get; init; }
}