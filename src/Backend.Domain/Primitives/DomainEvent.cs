
namespace Backend.Domain.Primitives;

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}