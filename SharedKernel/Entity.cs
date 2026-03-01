namespace SharedKernel;

public abstract class Entity<TId> : IHasDomainEvents, IEquatable<Entity<TId>> where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = []; 
        
    public TId Id { get; protected init; }

    protected Entity(TId id) => Id = id;

    protected Entity() {} //ef core
    
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseDomainEvent(DomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && other.Id.Equals(Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity<TId>)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}