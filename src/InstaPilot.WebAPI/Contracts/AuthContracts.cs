namespace InstaPilot.WebAPI.Contracts;

public sealed record RegisterRequest(string PhoneNumber, string Password, string DisplayName);
public sealed record LoginRequest(string PhoneNumber, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record VerifyPhoneRequest(string UserId, string Code);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, DateTime RefreshTokenExpiresAtUtc);
