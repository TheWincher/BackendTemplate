namespace Backend.Domain.Primitives;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
        => obj is Entity other && other.GetType() == GetType() && Id == other.Id;

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(Entity? left, Entity? right)
        => !(left == right);
}