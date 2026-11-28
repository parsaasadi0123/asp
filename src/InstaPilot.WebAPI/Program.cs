using System.Text;
using System.Threading.RateLimiting;
using InstaPilot.Application;
using InstaPilot.Infrastructure;
using InstaPilot.Infrastructure.Identity;
using InstaPilot.WebAPI.Middleware;
using InstaPilot.WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddScoped<IPhoneVerificationSender, DevelopmentPhoneVerificationSender>();
}
else
{
    builder.Services.AddOptions<SmsIrOptions>()
        .Bind(builder.Configuration.GetSection(SmsIrOptions.SectionName))
        .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "SmsIr:ApiKey is required outside Development.")
        .Validate(options => options.TemplateId > 0, "SmsIr:TemplateId must be a positive integer.")
        .Validate(options => !string.IsNullOrWhiteSpace(options.CodeParameterName), "SmsIr:CodeParameterName is required.")
        .ValidateOnStart();
    builder.Services.AddHttpClient<IPhoneVerificationSender, SmsIrPhoneVerificationSender>(client =>
    {
        client.BaseAddress = new Uri("https://api.sms.ir/");
        client.Timeout = TimeSpan.FromSeconds(15);
    });
}
builder.Services.AddControllers();
builder.Services.AddDataProtection();
builder.Services.Configure<MetaOAuthOptions>(builder.Configuration.GetSection(MetaOAuthOptions.SectionName));
builder.Services.AddHttpClient("MetaGraph", client => client.Timeout = TimeSpan.FromSeconds(20));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.SigningKey) || Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
    throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience are required.");
if (jwt.AccessTokenMinutes is < 1 or > 60 || jwt.RefreshTokenDays is < 1 or > 90)
    throw new InvalidOperationException("JWT token lifetimes are outside the allowed range.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer,
        ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.Run();

public partial class Program { }
