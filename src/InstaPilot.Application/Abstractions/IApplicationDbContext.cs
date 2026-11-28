using InstaPilot.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;
namespace InstaPilot.Application.Abstractions;
public interface IApplicationDbContext
{
    DbSet<Workspace> Workspaces { get; }
    DbSet<WorkspaceMembership> WorkspaceMemberships { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
