using SharedKernel.Outbox;

namespace Tsutskiridze.TradeBuddy.Infrastructure.Persistence.Outbox;

internal class OutboxWriter : IOutbox
{
    private readonly AppDbContext _db;

    public OutboxWriter(AppDbContext db)
    {
        _db = db;
    }

    public void Add(OutboxMessage message)
    {
        _db.OutboxMessages.Add(message);
    }

    public Task Save()
    {
        return Task.CompletedTask;
    }
}