namespace InstaPilot.WebAPI.Services;

public sealed class MetaOAuthOptions
{
    public const string SectionName = "MetaOAuth";
    public string AppId { get; init; } = string.Empty;
    public string AppSecret { get; init; } = string.Empty;
    public string RedirectUri { get; init; } = string.Empty;
    public string GraphApiVersion { get; init; } = "v24.0";
    public string FrontendBaseUrl { get; init; } = string.Empty;
}
