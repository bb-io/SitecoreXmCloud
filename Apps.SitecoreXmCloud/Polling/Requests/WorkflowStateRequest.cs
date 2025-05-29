using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Sitecore.Polling.Requests;

public class WorkflowStateRequest
{
    [Display("Workflow state ID"), DataSource(typeof(WorkflowStateDataHandler))]
    public string WorkflowStateId { get; set; } = string.Empty;
}