using InstaPilot.Infrastructure.Identity;
using InstaPilot.Infrastructure.Persistence;
using InstaPilot.WebAPI.Contracts;
using InstaPilot.WebAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstaPilot.WebAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    InstaPilotDbContext db,
    TokenService tokenService,
    IPhoneVerificationSender verificationSender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var phone = NormalizePhone(request.PhoneNumber);
        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 100)
            return ValidationProblem("Display name is required and must be at most 100 characters.");
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = phone, PhoneNumber = phone, DisplayName = request.DisplayName.Trim() };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(x => new { x.Code, x.Description }) });

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, phone);
        await verificationSender.SendAsync(phone, code, cancellationToken);
        return Accepted(new { userId = user.Id, message = "Verification code sent." });
    }

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone(VerifyPhoneRequest request)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            return BadRequest(new { message = "Invalid user id." });
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound();
        var result = await userManager.ChangePhoneNumberAsync(user, user.PhoneNumber!, request.Code);
        if (!result.Succeeded) return BadRequest(new { message = "Verification code is invalid or expired." });
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(NormalizePhone(request.PhoneNumber));
        if (user is null || !user.IsActive) return Unauthorized();
        var signIn = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signIn.Succeeded) return Unauthorized(new { message = "Invalid credentials or phone number is not verified." });
        return Ok(await IssueTokensAsync(user, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = TokenService.HashToken(request.RefreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        var now = DateTime.UtcNow;
        if (stored is null || !stored.IsActive(now)) return Unauthorized();
        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive || !user.PhoneNumberConfirmed) return Unauthorized();

        var pair = tokenService.Create(user.Id, user.PhoneNumber!, user.DisplayName);
        stored.RevokedAtUtc = now;
        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = TokenService.HashToken(pair.RefreshToken),
            CreatedAtUtc = now, ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        };
        stored.ReplacedByTokenId = replacement.Id;
        db.RefreshTokens.Add(replacement);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new AuthResponse(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAtUtc, pair.RefreshTokenExpiresAtUtc));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        var hash = TokenService.HashToken(request.RefreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (stored is not null && stored.RevokedAtUtc is null)
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        return NoContent();
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var pair = tokenService.Create(user.Id, user.PhoneNumber!, user.DisplayName);
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = TokenService.HashToken(pair.RefreshToken),
            CreatedAtUtc = DateTime.UtcNow, ExpiresAtUtc = pair.RefreshTokenExpiresAtUtc,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
        await db.SaveChangesAsync(cancellationToken);
        return new AuthResponse(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAtUtc, pair.RefreshTokenExpiresAtUtc);
    }

    private static string NormalizePhone(string phone) => phone.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);
}
