namespace SharedKernel.Outbox;

public interface IOutbox
{
    void Add(OutboxMessage message);

    Task Save();
}
