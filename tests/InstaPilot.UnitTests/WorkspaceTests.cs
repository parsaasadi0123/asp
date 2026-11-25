using InstaPilot.Domain.Workspaces;
using Xunit;

namespace InstaPilot.UnitTests;

public sealed class WorkspaceTests
{
    [Fact]
    public void Constructor_NormalizesSlugAndTrimsName()
    {
        var workspace = new Workspace(Guid.NewGuid(), "  My Brand  ", " My-Brand ", Guid.NewGuid());
        Assert.Equal("My Brand", workspace.Name);
        Assert.Equal("my-brand", workspace.Slug);
    }

    [Fact]
    public void Constructor_RejectsInvalidSlug()
    {
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.NewGuid(), "Brand", "invalid slug", Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_RejectsEmptyOwnerId()
    {
        Assert.Throws<ArgumentException>(() => new Workspace(Guid.NewGuid(), "Brand", "brand", Guid.Empty));
    }

    [Fact]
    public void OwnerCannotBeDemotedWithoutOwnershipTransfer()
    {
        var membership = new WorkspaceMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), WorkspaceRole.Owner);
        Assert.Throws<InvalidOperationException>(() => membership.ChangeRole(WorkspaceRole.Admin));
    }

    [Fact]
    public void NonOwnerMembershipCanChangeRole()
    {
        var membership = new WorkspaceMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), WorkspaceRole.Editor);
        membership.ChangeRole(WorkspaceRole.Analyst);
        Assert.Equal(WorkspaceRole.Analyst, membership.Role);
    }
    [Fact]
    public void RelinquishOwnership_TransfersCurrentOwnerToAdmin()
    {
        var membership = new WorkspaceMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), WorkspaceRole.Owner);
        membership.RelinquishOwnership();
        Assert.Equal(WorkspaceRole.Admin, membership.Role);
    }

    [Fact]
    public void NonOwnerCannotRelinquishOwnership()
    {
        var membership = new WorkspaceMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), WorkspaceRole.Editor);
        Assert.Throws<InvalidOperationException>(() => membership.RelinquishOwnership());
    }

}
