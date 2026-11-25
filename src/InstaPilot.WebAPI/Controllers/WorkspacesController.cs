using System.Security.Claims;
using InstaPilot.Domain.Workspaces;
using InstaPilot.Infrastructure.Identity;
using InstaPilot.Infrastructure.Persistence;
using InstaPilot.WebAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaPilot.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/workspaces")]
public sealed class WorkspacesController(
    InstaPilotDbContext db,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkspaceResponse>>> List(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var items = await db.WorkspaceMemberships.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(db.Workspaces.AsNoTracking(), m => m.WorkspaceId, w => w.Id,
                (m, w) => new WorkspaceResponse(w.Id, w.Name, w.Slug, m.Role.ToString(), w.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<WorkspaceResponse>> Create(CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var workspace = new Workspace(Guid.NewGuid(), request.Name, request.Slug, userId);
        if (await db.Workspaces.AnyAsync(x => x.Slug == workspace.Slug, cancellationToken))
            return Conflict(new { message = "Workspace slug is already in use." });

        var membership = new WorkspaceMembership(Guid.NewGuid(), workspace.Id, userId, WorkspaceRole.Owner);
        db.Workspaces.Add(workspace);
        db.WorkspaceMemberships.Add(membership);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), new WorkspaceResponse(workspace.Id, workspace.Name, workspace.Slug, membership.Role.ToString(), workspace.CreatedAtUtc));
    }

    [HttpPost("{workspaceId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid workspaceId, AddWorkspaceMemberRequest request, CancellationToken cancellationToken)
    {
        var access = await GetMembershipAsync(workspaceId, cancellationToken);
        if (access is null) return NotFound();
        if (access.Role is not (WorkspaceRole.Owner or WorkspaceRole.Admin)) return Forbid();
        if (!Enum.IsDefined(typeof(WorkspaceRole), request.Role) || request.Role == (int)WorkspaceRole.Owner)
            return BadRequest(new { message = "Invalid member role." });

        var user = await userManager.FindByNameAsync(request.PhoneNumber.Trim().Replace(" ", string.Empty).Replace("-", string.Empty));
        if (user is null) return NotFound(new { message = "User must register before being added." });
        var exists = await db.WorkspaceMemberships.AnyAsync(x => x.WorkspaceId == workspaceId && x.UserId == user.Id, cancellationToken);
        if (exists) return Conflict(new { message = "User is already a workspace member." });
        db.WorkspaceMemberships.Add(new WorkspaceMembership(Guid.NewGuid(), workspaceId, user.Id, (WorkspaceRole)request.Role));
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("{workspaceId:guid}/members")]
    public async Task<IActionResult> Members(Guid workspaceId, CancellationToken cancellationToken)
    {
        var access = await GetMembershipAsync(workspaceId, cancellationToken);
        if (access is null) return NotFound();
        var members = await db.WorkspaceMemberships.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .Join(db.Users.AsNoTracking(), m => m.UserId, u => u.Id,
                (m, u) => new { u.Id, u.DisplayName, u.PhoneNumber, Role = m.Role.ToString(), m.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        return Ok(members);
    }

    [HttpPatch("{workspaceId:guid}/members/{userId:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid workspaceId, Guid userId, ChangeWorkspaceRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = await GetMembershipAsync(workspaceId, cancellationToken);
        if (actor is null) return NotFound();
        if (actor.Role is not (WorkspaceRole.Owner or WorkspaceRole.Admin)) return Forbid();
        if (!Enum.IsDefined(typeof(WorkspaceRole), request.Role) || request.Role == (int)WorkspaceRole.Owner)
            return BadRequest(new { message = "Invalid member role." });
        var target = await db.WorkspaceMemberships.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (target is null) return NotFound();
        if (target.Role == WorkspaceRole.Owner) return Conflict(new { message = "Transfer ownership before changing the owner's role." });
        if (actor.Role != WorkspaceRole.Owner && target.Role == WorkspaceRole.Admin) return Forbid();
        target.ChangeRole((WorkspaceRole)request.Role);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{workspaceId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
    {
        var actor = await GetMembershipAsync(workspaceId, cancellationToken);
        if (actor is null) return NotFound();
        var target = await db.WorkspaceMemberships.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (target is null) return NotFound();
        if (target.Role == WorkspaceRole.Owner) return Conflict(new { message = "Transfer ownership before removing the owner." });
        if (actor.Role != WorkspaceRole.Owner && actor.Role != WorkspaceRole.Admin) return Forbid();
        if (actor.Role == WorkspaceRole.Admin && target.Role == WorkspaceRole.Admin) return Forbid();
        if (actor.UserId != userId && actor.Role != WorkspaceRole.Owner && target.Role == WorkspaceRole.Owner) return Forbid();
        db.WorkspaceMemberships.Remove(target);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }

    private Task<WorkspaceMembership?> GetMembershipAsync(Guid workspaceId, CancellationToken cancellationToken) =>
        db.WorkspaceMemberships.SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == CurrentUserId(), cancellationToken);
}
