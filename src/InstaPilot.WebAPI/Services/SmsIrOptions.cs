namespace InstaPilot.WebAPI.Services;
public sealed class SmsIrOptions
{
    public const string SectionName = "SmsIr";
    public string ApiKey { get; set; } = string.Empty;
    public int TemplateId { get; set; }
    public string CodeParameterName { get; set; } = "CODE";
}
