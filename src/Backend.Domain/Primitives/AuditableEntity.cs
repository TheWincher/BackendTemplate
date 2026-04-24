namespace Backend.Domain.Primitives;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    public void SetCreatedAt(DateTime date) => CreatedAt = date;
    public void SetUpdatedAt(DateTime date) => UpdatedAt = date;
}