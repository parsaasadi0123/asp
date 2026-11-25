namespace InstaPilot.WebAPI.Services;

public interface IPhoneVerificationSender
{
    Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken);
}
