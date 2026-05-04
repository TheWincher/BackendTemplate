using Backend.Domain.Primitives;

namespace Backend.Domain.Events;

public sealed record UserCreatedDomainEvent(Guid UserId) : DomainEvent;
public sealed record UserEmailChangedDomainEvent(Guid UserId, string NewEmail) : DomainEvent;