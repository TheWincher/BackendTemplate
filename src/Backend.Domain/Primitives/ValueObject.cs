namespace Backend.Domain.Primitives;

public abstract class ValueObject : IEquatable<ValueObject>
{
    // Chaque sous-classe déclare les propriétés qui définissent son identité
    protected abstract IEnumerable<object> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
        => obj is ValueObject vo && Equals(vo);

    public override int GetHashCode()
        => GetEqualityComponents()
            .Aggregate(0, (hash, obj) => HashCode.Combine(hash, obj));

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);
}