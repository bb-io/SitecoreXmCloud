using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses;

public class TemplateResponse
{
    [Display("Template ID")]
    public string TemplateId { get; set; } = string.Empty;

    [Display("Template name")]
    public string TemplateName { get; set; } = string.Empty;
}