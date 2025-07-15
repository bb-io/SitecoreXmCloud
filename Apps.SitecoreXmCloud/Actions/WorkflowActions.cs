using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.SitecoreXmCloud.Models.Requests.Workflows;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
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
        await ValidateWorkflowCommandExists(request.ItemId, request.Locale, request.Version, request.WorkflowCommandId);
        var bodyDictiory = new Dictionary<string, object>
        {
            { "commandId", request.WorkflowCommandId },
            { "comments", request.Comments ?? string.Empty },
            { "allowUi", request.AllowUi ?? true }
        };

        if (!string.IsNullOrEmpty(request.Version))
        {
            bodyDictiory.Add("version", request.Version);
        }
        
        if (!string.IsNullOrEmpty(request.Locale))
        {
            bodyDictiory.Add("locale", request.Locale);
        }

        var apiRequest = new SitecoreRequest("/ExecuteCommand", Method.Post, Creds)
            .AddQueryParameter("itemId", request.ItemId)
            .AddJsonBody(bodyDictiory);
        
        return await Client.ExecuteWithErrorHandling<ExecuteCommandResponse>(apiRequest);
    }
    
    private async Task ValidateWorkflowCommandExists(string itemId, string? locale, string? version, string commandId)
    {
        var itemActions = new ItemsActions(InvocationContext);
        var itemWorkflow = await itemActions.GetWorkflowState(new()
        {
            ItemId = itemId,
            Locale = locale,
            Version = version
        });
        
        if(itemWorkflow.Commands.All(x => x.CommandId.Trim('{', '}') != commandId.Trim('{', '}')))
        {
            var commandsString = string.Join(", ", itemWorkflow.Commands.Select(c => $"{c.CommandId} - {c.CommandName}"));
            throw new PluginMisconfigurationException($"Workflow command with ID {commandId} does not exist for item {itemId}. " +
                                                      $"Possible commands: {commandsString}");
        }
    }
}