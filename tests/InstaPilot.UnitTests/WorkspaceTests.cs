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
}
