using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using InstaPilot.Infrastructure.Persistence;
using InstaPilot.WebAPI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InstaPilot.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    public TestPhoneSender PhoneSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=ignored.db",
                ["Jwt:Issuer"] = "InstaPilot.Tests",
                ["Jwt:Audience"] = "InstaPilot.Tests.Clients",
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-bytes-long",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "30",
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<InstaPilotDbContext>();
            services.RemoveAll<DbContextOptions<InstaPilotDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<InstaPilotDbContext>>();
            services.AddDbContext<InstaPilotDbContext>(options => options.UseSqlite(_connection));
            services.RemoveAll<IPhoneVerificationSender>();
            services.AddSingleton<IPhoneVerificationSender>(PhoneSender);
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<InstaPilotDbContext>().Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public sealed class TestPhoneSender : IPhoneVerificationSender
{
    public string? LastCode { get; private set; }
    public Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        LastCode = code;
        return Task.CompletedTask;
    }
}

public sealed class AuthenticationAndWorkspaceFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_Verify_Login_Refresh_And_IsolateWorkspaceMembers()
    {
        await factory.InitializeDatabaseAsync();
        var client = factory.CreateClient();

        const string ownerPhone = "+989121234501";
        var ownerRegistration = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { phoneNumber = ownerPhone, password = "SecurePass123", displayName = "Owner" });
        Assert.Equal(HttpStatusCode.Accepted, ownerRegistration.StatusCode);
        var ownerUserId = (await ownerRegistration.Content.ReadFromJsonAsync<UserCreated>())!.UserId;
        Assert.NotNull(factory.PhoneSender.LastCode);

        var verifyOwner = await client.PostAsJsonAsync("/api/v1/auth/verify-phone",
            new { userId = ownerUserId, code = factory.PhoneSender.LastCode });
        Assert.Equal(HttpStatusCode.NoContent, verifyOwner.StatusCode);

        var ownerLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { phoneNumber = ownerPhone, password = "SecurePass123" });
        Assert.Equal(HttpStatusCode.OK, ownerLogin.StatusCode);
        var ownerTokens = (await ownerLogin.Content.ReadFromJsonAsync<TokenResult>())!;
        Assert.False(string.IsNullOrWhiteSpace(ownerTokens.AccessToken));

        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = ownerTokens.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        var refreshed = (await refresh.Content.ReadFromJsonAsync<TokenResult>())!;
        Assert.NotEqual(ownerTokens.RefreshToken, refreshed.RefreshToken);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        var createWorkspace = await client.PostAsJsonAsync("/api/v1/workspaces",
            new { name = "Brand workspace", slug = "brand-workspace" });
        Assert.Equal(HttpStatusCode.Created, createWorkspace.StatusCode);
        var workspace = (await createWorkspace.Content.ReadFromJsonAsync<WorkspaceResult>())!;

        const string memberPhone = "+989121234502";
        var memberRegistration = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { phoneNumber = memberPhone, password = "SecurePass123", displayName = "Member" });
        Assert.Equal(HttpStatusCode.Accepted, memberRegistration.StatusCode);
        var memberUserId = (await memberRegistration.Content.ReadFromJsonAsync<UserCreated>())!.UserId;
        var memberCode = factory.PhoneSender.LastCode;
        var verifyMember = await client.PostAsJsonAsync("/api/v1/auth/verify-phone",
            new { userId = memberUserId, code = memberCode });
        Assert.Equal(HttpStatusCode.NoContent, verifyMember.StatusCode);

        var addMember = await client.PostAsJsonAsync($"/api/v1/workspaces/{workspace.Id}/members",
            new { phoneNumber = memberPhone, role = 3 });
        Assert.Equal(HttpStatusCode.NoContent, addMember.StatusCode);

        var memberLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { phoneNumber = memberPhone, password = "SecurePass123" });
        Assert.Equal(HttpStatusCode.OK, memberLogin.StatusCode);
        var memberTokens = (await memberLogin.Content.ReadFromJsonAsync<TokenResult>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberTokens.AccessToken);
        var memberWorkspaces = await client.GetFromJsonAsync<WorkspaceResult[]>("/api/v1/workspaces");
        Assert.Single(memberWorkspaces!);

        const string strangerPhone = "+989121234503";
        var strangerRegistration = await client.PostAsJsonAsync("/api/v1/auth/register",
            new { phoneNumber = strangerPhone, password = "SecurePass123", displayName = "Stranger" });
        Assert.Equal(HttpStatusCode.Accepted, strangerRegistration.StatusCode);
        var strangerId = (await strangerRegistration.Content.ReadFromJsonAsync<UserCreated>())!.UserId;
        var strangerCode = factory.PhoneSender.LastCode;
        var verifyStranger = await client.PostAsJsonAsync("/api/v1/auth/verify-phone",
            new { userId = strangerId, code = strangerCode });
        Assert.Equal(HttpStatusCode.NoContent, verifyStranger.StatusCode);
        var strangerLogin = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { phoneNumber = strangerPhone, password = "SecurePass123" });
        Assert.Equal(HttpStatusCode.OK, strangerLogin.StatusCode);
        var strangerTokens = (await strangerLogin.Content.ReadFromJsonAsync<TokenResult>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", strangerTokens.AccessToken);
        var strangerWorkspaces = await client.GetFromJsonAsync<WorkspaceResult[]>("/api/v1/workspaces");
        Assert.Empty(strangerWorkspaces!);
    }

    private sealed record UserCreated(Guid UserId);
    private sealed record TokenResult(string AccessToken, string RefreshToken);
    private sealed record WorkspaceResult(Guid Id, string Name, string Slug, string Role, DateTime CreatedAtUtc);
}