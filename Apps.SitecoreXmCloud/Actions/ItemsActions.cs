using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Requests.Item;
using Apps.Sitecore.Models.Responses.Item;
using Apps.SitecoreXmCloud.Models.Responses.Workflows;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList]
public class ItemsActions(InvocationContext invocationContext) : SitecoreInvocable(invocationContext)
{
    [Action("Search items", Description = "Search items based on provided criterias")]
    public async Task<ListItemsResponse> SearchItems([ActionParameter] SearchItemsRequest input)
    {
        var endpoint = "/Search".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Get, Creds);

        var items = await Client.Paginate<ItemEntity>(request);

        var latestItems = items
                   .GroupBy(item => item.Id)
                   .Select(g => g.OrderByDescending(item =>
                   {
                       int versionNumber;
                       return int.TryParse(item.Version, out versionNumber) ? versionNumber : 0;
                   }).FirstOrDefault())
                   .ToList();

        return new ListItemsResponse(latestItems);
    }

    [Action("Get workflow state", Description = "Get workflow state of an item")]
    public async Task<ItemWorkflowResponse> GetWorkflowState([ActionParameter] ItemContentRequest itemRequest)
    {
        var request = new SitecoreRequest("/GetItemWorkflow", Method.Get, Creds)
            .AddQueryParameter("itemId", itemRequest.ItemId);
        
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
}