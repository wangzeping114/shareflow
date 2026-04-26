namespace ShareFlow.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}
