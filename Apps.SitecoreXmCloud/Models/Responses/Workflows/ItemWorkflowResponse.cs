using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses.Workflows;

public class ItemWorkflowResponse
{
    [Display("Workflow")]
    public WorkflowResponse Workflow { get; set; } = new();

    [Display("Current state")]
    public WorkflowState CurrentState { get; set; } = new();

    [Display("Commands")]
    public List<WorkflowCommand> Commands { get; set; } = new();
}