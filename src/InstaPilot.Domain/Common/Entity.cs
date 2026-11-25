namespace InstaPilot.Domain.Common;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }
    public bool IsDeleted { get; protected set; }
    protected Entity() { }
    protected Entity(TId id) { Id = id; CreatedAtUtc = DateTime.UtcNow; }
    public void MarkUpdated(DateTime utcNow) => UpdatedAtUtc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    public void SoftDelete(DateTime utcNow) { IsDeleted = true; MarkUpdated(utcNow); }
}
