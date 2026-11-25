using System.Text.Json;
using Microsoft.Extensions.Options;
namespace InstaPilot.WebAPI.Services;
public sealed class SmsIrPhoneVerificationSender(
    HttpClient httpClient,
    IOptions<SmsIrOptions> options,
    ILogger<SmsIrPhoneVerificationSender> logger) : IPhoneVerificationSender
{
    private readonly SmsIrOptions _options = options.Value;
    public async Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/send/verify")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new
            {
                mobile = phoneNumber,
                templateId = _options.TemplateId,
                parameters = new[] { new { name = _options.CodeParameterName, value = code } }
            })
        };
        request.Headers.Add("X-API-KEY", _options.ApiKey);
        request.Headers.Accept.ParseAdd("application/json");
        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(payload);
        if (!document.RootElement.TryGetProperty("status", out var status) || status.GetInt32() != 1)
        {
            var providerMessage = document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString() : "Unknown SMS.ir provider error.";
            logger.LogError("SMS.ir rejected verification delivery. Provider message: {ProviderMessage}", providerMessage);
            throw new HttpRequestException("SMS provider rejected the verification request.");
        }
    }
}
