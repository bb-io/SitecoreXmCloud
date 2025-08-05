using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Requests.Item;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.DataSourceHandlers;

public class WorkflowCommandDataHandler(InvocationContext invocationContext, [ActionParameter] ItemContentRequest itemContentRequest) 
    : SitecoreInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(itemContentRequest.ItemId))
        {
            throw new ArgumentException("Please provide an item ID first to retrieve workflow states.");
        }

        var request = new SitecoreRequest("/GetItemWorkflow", Method.Get, Creds)
            .AddQueryParameter("itemId", itemContentRequest.ItemId);

        if(string.IsNullOrEmpty(itemContentRequest.Locale) == false)
        {
            request.AddQueryParameter("locale", itemContentRequest.Locale);
        }

        if(string.IsNullOrEmpty(itemContentRequest.Version) == false)
        {
            request.AddQueryParameter("version", itemContentRequest.Version);
        }

        var response = await Client.ExecuteWithErrorHandling<ItemWorkflowResponse>(request);
        return response.Commands
            .Where(x => context.SearchString == null || x.CommandName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(command => new DataSourceItem(command.CommandId, command.CommandName));
    }
}