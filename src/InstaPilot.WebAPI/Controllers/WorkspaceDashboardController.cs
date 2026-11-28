using System.Security.Claims;
using InstaPilot.Domain.Workspaces;
using InstaPilot.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaPilot.WebAPI.Controllers;

public sealed record WorkspaceDashboardResponse(
    Guid WorkspaceId,
    string WorkspaceName,
    string CurrentUserRole,
    int MemberCount,
    int OwnerCount,
    int AdminCount,
    int ContentManagerCount,
    int EditorCount,
    int AnalystCount,
    int ConnectedMetaAccounts);

[ApiController]
[Authorize]
[Route("api/v1/workspaces/{workspaceId:guid}/dashboard")]
public sealed class WorkspaceDashboardController(InstaPilotDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<WorkspaceDashboardResponse>> Get(Guid workspaceId, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) return Unauthorized();

        var membership = await db.WorkspaceMemberships.AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (membership is null) return NotFound();

        var workspace = await db.Workspaces.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == workspaceId, cancellationToken);
        if (workspace is null) return NotFound();

        var roles = await db.WorkspaceMemberships.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .GroupBy(x => x.Role)
            .Select(group => new { Role = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, cancellationToken);

        var metaCount = await db.MetaAccounts.AsNoTracking()
            .CountAsync(x => x.WorkspaceId == workspaceId, cancellationToken);

        int Count(WorkspaceRole role) => roles.TryGetValue(role, out var count) ? count : 0;
        return Ok(new WorkspaceDashboardResponse(
            workspace.Id, workspace.Name, membership.Role.ToString(),
            roles.Values.Sum(), Count(WorkspaceRole.Owner), Count(WorkspaceRole.Admin),
            Count(WorkspaceRole.ContentManager), Count(WorkspaceRole.Editor), Count(WorkspaceRole.Analyst),
            metaCount));
    }
}
