using System.Reflection;
using InstaPilot.Domain.Workspaces;
using Xunit;
namespace InstaPilot.ArchitectureTests;
public sealed class DependencyRulesTests
{
    [Fact]
    public void Domain_DoesNotDependOnInfrastructureOrWebFrameworks()
    {
        var references = typeof(Workspace).Assembly.GetReferencedAssemblies().Select(x => x.Name ?? string.Empty).ToArray();
        Assert.DoesNotContain(references, x => x.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, x => x.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, x => x.StartsWith("InstaPilot.Infrastructure", StringComparison.Ordinal));
    }
}
