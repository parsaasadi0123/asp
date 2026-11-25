using InstaPilot.Application.Abstractions;
using InstaPilot.Domain.Workspaces;
using InstaPilot.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace InstaPilot.Infrastructure.Persistence;
public sealed class InstaPilotDbContext(DbContextOptions<InstaPilotDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IApplicationDbContext
{
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();
    protected override void OnModelCreating(ModelBuilder builder) { base.OnModelCreating(builder); builder.ApplyConfigurationsFromAssembly(typeof(InstaPilotDbContext).Assembly); }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<InstaPilot.Domain.Common.Entity<Guid>>())
            if (entry.State == EntityState.Modified) entry.Entity.MarkUpdated(now);
        return base.SaveChangesAsync(cancellationToken);
    }
}
