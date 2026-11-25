namespace InstaPilot.WebAPI.Contracts;

public sealed record CreateWorkspaceRequest(string Name, string Slug);
public sealed record WorkspaceResponse(Guid Id, string Name, string Slug, string Role, DateTime CreatedAtUtc);
public sealed record AddWorkspaceMemberRequest(string PhoneNumber, int Role);
public sealed record ChangeWorkspaceRoleRequest(int Role);
