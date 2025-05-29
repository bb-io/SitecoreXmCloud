using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses.Workflows;

public class WorkflowState
{
    [Display("State ID")]
    public string StateId { get; set; } = string.Empty;

    [Display("State name")]
    public string StateName { get; set; } = string.Empty;

    [Display("Is final state")]
    public bool FinalState { get; set; }
}