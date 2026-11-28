using InstaPilot.Domain.Common;

namespace InstaPilot.Domain.Meta;

public sealed class MetaAccount : Entity<Guid>
{
    public Guid WorkspaceId { get; private set; }
    public string MetaUserId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string ProtectedAccessToken { get; private set; } = string.Empty;
    public DateTime? AccessTokenExpiresAtUtc { get; private set; }
    public Guid ConnectedByUserId { get; private set; }

    private MetaAccount() { }

    public MetaAccount(Guid id, Guid workspaceId, string metaUserId, string displayName,
        string protectedAccessToken, DateTime? accessTokenExpiresAtUtc, Guid connectedByUserId)
        : base(id)
    {
        if (workspaceId == Guid.Empty || connectedByUserId == Guid.Empty)
            throw new ArgumentException("Workspace and connected user are required.");
        if (string.IsNullOrWhiteSpace(metaUserId) || string.IsNullOrWhiteSpace(protectedAccessToken))
            throw new ArgumentException("Meta identity and protected token are required.");
        WorkspaceId = workspaceId;
        MetaUserId = metaUserId.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Meta account" : displayName.Trim();
        ProtectedAccessToken = protectedAccessToken;
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
        ConnectedByUserId = connectedByUserId;
    }

    public void UpdateProfile(string displayName, string protectedAccessToken, DateTime? expiresAtUtc)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? DisplayName : displayName.Trim();
        ProtectedAccessToken = protectedAccessToken;
        AccessTokenExpiresAtUtc = expiresAtUtc;
    }
}
