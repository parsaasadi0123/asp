namespace InstaPilot.WebAPI.Contracts;

public sealed record MetaAccountResponse(Guid Id, string MetaUserId, string DisplayName, DateTime? AccessTokenExpiresAtUtc);
