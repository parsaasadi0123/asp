using System.Text;
using InstaPilot.Application;
using InstaPilot.Infrastructure;
using InstaPilot.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console());
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers(); builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks(); builder.Services.AddProblemDetails(); builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var signingKey = builder.Configuration["Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32) throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");
var issuer = builder.Configuration["Jwt:Issuer"] ?? "InstaPilot"; var audience = builder.Configuration["Jwt:Audience"] ?? "InstaPilot.Clients";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = issuer, ValidateAudience = true, ValidAudience = audience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30) });
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? []).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
var app = builder.Build(); app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseCors("Frontend"); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); app.MapHealthChecks("/health");
app.Run();
public partial class Program { }
