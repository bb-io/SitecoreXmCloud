using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses.Workflows;

public class WorkflowResponse
{
    [Display("Workflow ID")]
    public string WorkflowId { get; set; } = string.Empty;

    [Display("Workflow name")]
    public string WorkflowName { get; set; } = string.Empty;

    [Display("States")]
    public List<WorkflowState> States { get; set; } = new();
}

