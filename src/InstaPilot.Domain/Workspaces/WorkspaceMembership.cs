using InstaPilot.Domain.Common;

namespace InstaPilot.Domain.Workspaces;

public sealed class WorkspaceMembership : Entity<Guid>
{
    public Guid WorkspaceId { get; private set; }
    public Guid UserId { get; private set; }
    public WorkspaceRole Role { get; private set; }

    private WorkspaceMembership() { }

    public WorkspaceMembership(Guid id, Guid workspaceId, Guid userId, WorkspaceRole role) : base(id)
    {
        if (workspaceId == Guid.Empty || userId == Guid.Empty)
            throw new ArgumentException("Workspace and user are required.");
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
    }

    public void ChangeRole(WorkspaceRole role)
    {
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        if (Role == WorkspaceRole.Owner && role != WorkspaceRole.Owner)
            throw new InvalidOperationException("Transfer ownership before changing the owner's role.");
        Role = role;
    }

    public void RelinquishOwnership()
    {
        if (Role != WorkspaceRole.Owner)
            throw new InvalidOperationException("Only the current owner can relinquish ownership.");
        Role = WorkspaceRole.Admin;
    }
}
