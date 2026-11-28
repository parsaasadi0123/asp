using InstaPilot.Domain.Meta;
using Xunit;

namespace InstaPilot.UnitTests;

public sealed class MetaOAuthStateTests
{
    [Fact]
    public void State_CanBeConsumedOnlyOnceBeforeExpiry()
    {
        var now = DateTime.UtcNow;
        var state = new MetaOAuthState(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddMinutes(5));

        Assert.True(state.TryConsume(now));
        Assert.False(state.TryConsume(now.AddSeconds(1)));
    }

    [Fact]
    public void ExpiredStateCannotBeConsumed()
    {
        var now = DateTime.UtcNow;
        var state = new MetaOAuthState(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), now.AddSeconds(-1));

        Assert.False(state.TryConsume(now));
    }

    [Fact]
    public void AccountRequiresProtectedTokenAndWorkspace()
    {
        Assert.Throws<ArgumentException>(() => new MetaAccount(Guid.NewGuid(), Guid.Empty,
            "meta-id", "Brand", "protected", null, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new MetaAccount(Guid.NewGuid(), Guid.NewGuid(),
            "meta-id", "Brand", "", null, Guid.NewGuid()));
    }
}
