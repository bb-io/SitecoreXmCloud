using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Requests.Item;
using Apps.Sitecore.Models.Responses.Item;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList]
public class ItemsActions : SitecoreInvocable
{
    public ItemsActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

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
                   }).First())
                   .ToList();

        return new ListItemsResponse(latestItems);
    }
}