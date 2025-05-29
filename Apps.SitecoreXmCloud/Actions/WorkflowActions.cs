using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.SitecoreXmCloud.Models.Requests.Workflows;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList]
public class WorkflowActions(InvocationContext invocationContext) : SitecoreInvocable(invocationContext)
{
    [Action("Search workflows", Description = "Retrieve a list of workflows")]
    public async Task<ListWorkflowsResponse> SearchWorkflows()
    {
        var request = new SitecoreRequest("/Workflows", Method.Get, Creds);
        var response = await Client.ExecuteWithErrorHandling<List<WorkflowResponse>>(request);
        return new ListWorkflowsResponse
        {
            Workflows = response
        };
    }

    [Action("Update workflow state", Description = "Update the workflow state of an item")]
    public async Task<ExecuteCommandResponse> UpdateWorkflowState([ActionParameter] UpdateWorkflowStateRequest request)
    {
        var body = new
        {
            commandId = request.WorkflowCommandId,
            comments = request.Comments ?? string.Empty,
            allowUi = request.AllowUi ?? true,
        };

        var apiRequest = new SitecoreRequest("/ExecuteCommand", Method.Post, Creds)
            .AddQueryParameter("itemId", request.ItemId)
            .AddJsonBody(body);
        
        return await Client.ExecuteWithErrorHandling<ExecuteCommandResponse>(apiRequest);
    }
}
