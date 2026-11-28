using InstaPilot.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace InstaPilot.Infrastructure.Persistence;
public sealed class InstaPilotDbContextFactory : IDesignTimeDbContextFactory<InstaPilotDbContext>
{
    public InstaPilotDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection before running EF Core design-time commands.");
        var options = new DbContextOptionsBuilder<InstaPilotDbContext>().UseSqlServer(connectionString).Options;
        return new InstaPilotDbContext(options);
    }
}
