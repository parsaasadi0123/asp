using InstaPilot.Domain.Common;

namespace InstaPilot.Domain.Meta;

public sealed class MetaOAuthState : Entity<Guid>
{
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }

    private MetaOAuthState() { }

    public MetaOAuthState(Guid id, Guid workspaceId, Guid userId, DateTime expiresAtUtc) : base(id)
    {
        if (workspaceId == Guid.Empty || userId == Guid.Empty)
            throw new ArgumentException("Workspace and user are required.");
        WorkspaceId = workspaceId;
        UserId = userId;
        ExpiresAtUtc = DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc);
    }

    public bool TryConsume(DateTime utcNow)
    {
        if (ConsumedAtUtc is not null || ExpiresAtUtc <= utcNow) return false;
        ConsumedAtUtc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        return true;
    }
}
