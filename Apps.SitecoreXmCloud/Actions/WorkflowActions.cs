using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Requests.Item;
using Apps.SitecoreXmCloud.Models.Requests.Workflows;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList("Workflows")]
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
    
    [Action("Get workflow state", Description = "Get workflow state of an item")]
    public async Task<ItemWorkflowResponse> GetWorkflowState([ActionParameter] ItemContentRequest itemRequest)
    {
        var request = new SitecoreRequest("/GetItemWorkflow", Method.Get, Creds)
            .AddQueryParameter("itemId", itemRequest.ContentId);
        
        if (!string.IsNullOrEmpty(itemRequest.Locale))
        {
            request.AddQueryParameter("locale", itemRequest.Locale);
        }
        
        if (!string.IsNullOrEmpty(itemRequest.Version))
        {
            request.AddQueryParameter("version", itemRequest.Version);
        }
        
        return await Client.ExecuteWithErrorHandling<ItemWorkflowResponse>(request);
    }

    [Action("Update workflow state", Description = "Update the workflow state of an item")]
    public async Task<ExecuteCommandResponse> UpdateWorkflowState([ActionParameter] UpdateWorkflowStateRequest request)
    {
        await ValidateWorkflowCommandExists(request.ContentId, request.Locale, request.Version, request.WorkflowCommandId);
        var bodyDictionary = new Dictionary<string, object>
        {
            { "commandId", request.WorkflowCommandId },
            { "comments", request.Comments ?? string.Empty },
            { "allowUi", request.AllowUi ?? true }
        };

        if (!string.IsNullOrEmpty(request.Version))
        {
            bodyDictionary.Add("version", request.Version);
        }
        
        if (!string.IsNullOrEmpty(request.Locale))
        {
            bodyDictionary.Add("locale", request.Locale);
        }

        var apiRequest = new SitecoreRequest("/ExecuteCommand", Method.Post, Creds)
            .AddQueryParameter("itemId", request.ContentId)
            .AddJsonBody(bodyDictionary);
        
        return await Client.ExecuteWithErrorHandling<ExecuteCommandResponse>(apiRequest);
    }
    
    private async Task ValidateWorkflowCommandExists(string itemId, string? locale, string? version, string commandId)
    {
        var itemWorkflow = await GetWorkflowState(new()
        {
            ContentId = itemId,
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