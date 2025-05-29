using Apps.Sitecore.DataSourceHandlers;
using Apps.Sitecore.Models.Requests.Item;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SitecoreXmCloud.Models.Requests.Workflows;

public class UpdateWorkflowStateRequest : ItemContentRequest
{
    [Display("Workflow command ID"), DataSource(typeof(WorkflowCommandDataHandler))]
    public string WorkflowCommandId { get; set; } = string.Empty;

    [Display("Comments")]
    public string? Comments { get; set; } = string.Empty;

    [Display("Allow UI")]
    public bool? AllowUi { get; set; }
}