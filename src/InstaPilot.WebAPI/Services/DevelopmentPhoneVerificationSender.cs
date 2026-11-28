using Microsoft.Extensions.Hosting;

namespace InstaPilot.WebAPI.Services;

/// <summary>Development-only OTP sink. Production must replace this with the approved SMS.ir adapter.</summary>
public sealed class DevelopmentPhoneVerificationSender(
    ILogger<DevelopmentPhoneVerificationSender> logger,
    IHostEnvironment environment) : IPhoneVerificationSender
{
    public Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            throw new InvalidOperationException("A production SMS provider is not configured.");
        logger.LogWarning("Development phone verification code for {PhoneNumber}: {Code}", phoneNumber, code);
        return Task.CompletedTask;
    }
}
