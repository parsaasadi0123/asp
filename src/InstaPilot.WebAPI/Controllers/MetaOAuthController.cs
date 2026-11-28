using System.Security.Claims;
using System.Text.Json;
using InstaPilot.Domain.Meta;
using InstaPilot.Domain.Workspaces;
using InstaPilot.Infrastructure.Persistence;
using InstaPilot.WebAPI.Contracts;
using InstaPilot.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InstaPilot.WebAPI.Controllers;

[ApiController]
[Route("api/v1/meta")]
public sealed class MetaOAuthController(
    InstaPilotDbContext db,
    IHttpClientFactory httpClientFactory,
    IDataProtectionProvider protectionProvider,
    IOptions<MetaOAuthOptions> options,
    ILogger<MetaOAuthController> logger) : ControllerBase
{
    private readonly MetaOAuthOptions _options = options.Value;
    private readonly IDataProtector _tokenProtector = protectionProvider.CreateProtector("InstaPilot.MetaAccessToken.v1");

    [Authorize]
    [HttpGet("connect/{workspaceId:guid}")]
    public async Task<IActionResult> Connect(Guid workspaceId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var membership = await db.WorkspaceMemberships.AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (membership is null) return NotFound();
        if (membership.Role is not (WorkspaceRole.Owner or WorkspaceRole.Admin)) return Forbid();

        if (string.IsNullOrWhiteSpace(_options.AppId) || string.IsNullOrWhiteSpace(_options.RedirectUri))
            return Problem("Meta OAuth is not configured for this environment.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var stateId = Guid.NewGuid();
        db.MetaOAuthStates.Add(new MetaOAuthState(stateId, workspaceId, userId, DateTime.UtcNow.AddMinutes(10)));
        await db.SaveChangesAsync(cancellationToken);

        var scopes = "pages_show_list,pages_read_engagement,instagram_basic,instagram_manage_insights,instagram_content_publish";
        var url = $"https://www.facebook.com/{Uri.EscapeDataString(_options.GraphApiVersion)}/dialog/oauth" +
                  $"?client_id={Uri.EscapeDataString(_options.AppId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                  $"&state={Uri.EscapeDataString(stateId.ToString("N"))}" +
                  $"&scope={Uri.EscapeDataString(scopes)}&response_type=code";
        return Ok(new { authorizationUrl = url });
    }

    [AllowAnonymous]
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state,
        [FromQuery] string? error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
            return BadRequest(new { message = "Meta authorization was cancelled or denied." });
        if (string.IsNullOrWhiteSpace(code) || !Guid.TryParseExact(state, "N", out var stateId))
            return BadRequest(new { message = "Invalid OAuth callback parameters." });
        if (string.IsNullOrWhiteSpace(_options.AppId) || string.IsNullOrWhiteSpace(_options.AppSecret) ||
            string.IsNullOrWhiteSpace(_options.RedirectUri))
            return Problem("Meta OAuth is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var now = DateTime.UtcNow;
        var consumed = await db.MetaOAuthStates
            .Where(x => x.Id == stateId && x.ConsumedAtUtc == null && x.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ConsumedAtUtc, now), cancellationToken);
        if (consumed != 1) return BadRequest(new { message = "OAuth state is expired or already used." });

        var oauthState = await db.MetaOAuthStates.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stateId, cancellationToken);
        if (oauthState is null) return BadRequest(new { message = "OAuth state was not found." });

        try
        {
            var client = httpClientFactory.CreateClient("MetaGraph");
            var tokenUrl = $"https://graph.facebook.com/{_options.GraphApiVersion}/oauth/access_token" +
                $"?client_id={Uri.EscapeDataString(_options.AppId)}" +
                $"&client_secret={Uri.EscapeDataString(_options.AppSecret)}" +
                $"&redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}" +
                $"&code={Uri.EscapeDataString(code)}";
            using var tokenResponse = await client.GetAsync(tokenUrl, cancellationToken);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Meta OAuth token exchange failed with status {StatusCode}.", tokenResponse.StatusCode);
                return BadRequest(new { message = "Meta authorization code could not be exchanged." });
            }
            var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<MetaTokenPayload>(cancellationToken: cancellationToken);
            if (tokenPayload is null || string.IsNullOrWhiteSpace(tokenPayload.AccessToken))
                return BadRequest(new { message = "Meta returned an invalid token response." });

            var profileUrl = $"https://graph.facebook.com/{_options.GraphApiVersion}/me?fields=id,name&access_token={Uri.EscapeDataString(tokenPayload.AccessToken)}";
            using var profileResponse = await client.GetAsync(profileUrl, cancellationToken);
            if (!profileResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Meta profile lookup failed with status {StatusCode}.", profileResponse.StatusCode);
                return BadRequest(new { message = "Could not retrieve the authorized Meta profile." });
            }
            var profile = await profileResponse.Content.ReadFromJsonAsync<MetaProfilePayload>(cancellationToken: cancellationToken);
            if (profile is null || string.IsNullOrWhiteSpace(profile.Id))
                return BadRequest(new { message = "Meta profile response is incomplete." });

            var protectedToken = _tokenProtector.Protect(tokenPayload.AccessToken);
            var expiry = tokenPayload.ExpiresIn > 0 ? now.AddSeconds(tokenPayload.ExpiresIn) : (DateTime?)null;
            var existing = await db.MetaAccounts.SingleOrDefaultAsync(
                x => x.WorkspaceId == oauthState.WorkspaceId && x.MetaUserId == profile.Id, cancellationToken);
            if (existing is null)
            {
                db.MetaAccounts.Add(new MetaAccount(Guid.NewGuid(), oauthState.WorkspaceId, profile.Id,
                    profile.Name ?? "Meta account", protectedToken, expiry, oauthState.UserId));
            }
            else
            {
                existing.UpdateProfile(profile.Name ?? existing.DisplayName, protectedToken, expiry);
            }
            await db.SaveChangesAsync(cancellationToken);
            return Redirect($"{_options.FrontendBaseUrl.TrimEnd('/')}/settings/channels?meta=connected");
        }
        catch (HttpRequestException)
        {
            logger.LogError("Meta OAuth network request failed.");
            return Problem("Meta service could not be reached.", statusCode: StatusCodes.Status502BadGateway);
        }
        catch (JsonException)
        {
            logger.LogWarning("Meta OAuth returned malformed JSON.");
            return BadRequest(new { message = "Meta returned an invalid response." });
        }
    }

    [Authorize]
    [HttpGet("accounts/{workspaceId:guid}")]
    public async Task<ActionResult<IReadOnlyList<MetaAccountResponse>>> Accounts(Guid workspaceId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var membership = await db.WorkspaceMemberships.AsNoTracking()
            .SingleOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (membership is null) return NotFound();
        var accounts = await db.MetaAccounts.AsNoTracking().Where(x => x.WorkspaceId == workspaceId)
            .Select(x => new MetaAccountResponse(x.Id, x.MetaUserId, x.DisplayName, x.AccessTokenExpiresAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(accounts);
    }

    [Authorize]
    [HttpDelete("accounts/{workspaceId:guid}/{accountId:guid}")]
    public async Task<IActionResult> Disconnect(Guid workspaceId, Guid accountId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var membership = await db.WorkspaceMemberships.SingleOrDefaultAsync(
            x => x.WorkspaceId == workspaceId && x.UserId == userId, cancellationToken);
        if (membership is null) return NotFound();
        if (membership.Role is not (WorkspaceRole.Owner or WorkspaceRole.Admin)) return Forbid();
        var account = await db.MetaAccounts.SingleOrDefaultAsync(
            x => x.Id == accountId && x.WorkspaceId == workspaceId, cancellationToken);
        if (account is null) return NotFound();
        account.SoftDelete(DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedAccessException();
    }

    private sealed record MetaTokenPayload(string AccessToken, int ExpiresIn);
    private sealed record MetaProfilePayload(string Id, string? Name);
}
