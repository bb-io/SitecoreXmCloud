using Apps.SitecoreXmCloud.Models.Responses;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;

namespace Apps.Sitecore.Models.Entities;

public class ItemEntity
{
    [Display("Item ID")]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    [Display("Full path")]
    public string FullPath { get; set; } = string.Empty;

    [Display("Created at")]
    public DateTime CreatedAt { get; set; }

    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; }

    public IEnumerable<string> Sites { get; set; } = new List<string>();

    public TemplateResponse Template { get; set; } = new();

    [Display("Created by")]
    public string CreatedBy { get; set; } = string.Empty;

    [Display("Updated by")]
    public string UpdatedBy { get; set; } = string.Empty;

    [Display("Workflow")]
    public WorkflowResponse? Workflow { get; set; }

    [Display("Current workflow state")]
    public WorkflowState? CurrentState { get; set; }
}