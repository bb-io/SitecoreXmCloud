using Blackbird.Applications.Sdk.Common;

namespace Apps.SitecoreXmCloud.Models.Responses.Workflows;

public class ExecuteCommandResponse
{
    public string Message { get; set; } = string.Empty;
    
    [Display("Next step ID")]
    public string NextStepId { get; set; } = string.Empty;

    public bool Succeeded { get; set; }

    public bool Completed { get; set; }
}
