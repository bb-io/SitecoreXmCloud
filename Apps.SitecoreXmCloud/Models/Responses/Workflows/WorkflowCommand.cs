using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses.Workflows;

public class WorkflowCommand
{
    [Display("Command ID")]
    public string CommandId { get; set; } = string.Empty;
    
    [Display("Command name")]
    public string CommandName { get; set; } = string.Empty;
    
    [Display("Has UI")]
    public bool HasUi { get; set; }
    
    [Display("Suppress comment")]
    public bool SuppressComment { get; set; }
}