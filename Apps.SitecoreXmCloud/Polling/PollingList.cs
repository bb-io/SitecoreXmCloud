using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;

namespace Apps.Sitecore.Polling;

[PollingEventList]
public class PollingList : SitecoreInvocable
{
    public PollingList(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [PollingEvent("On items created", "On new items created")]
    public Task<PollingEventResponse<DateMemory, ListItemsResponse>> OnItemsCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
        => HandleItemsPolling(request,
             $"locale={input.Locale}&rootPath={input.RootPath}&createdAt={request.Memory?.LastInteractionDate}&createdOperation=GreaterOrEqual");

    [PollingEvent("On items updated", "On any items updated")]
    public Task<PollingEventResponse<DateMemory, ListItemsResponse>> OnItemsUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
        => HandleItemsPolling(request,
            $"locale={input.Locale}&rootPath={input.RootPath}&updatedAt={request.Memory?.LastInteractionDate}&updatedOperation=GreaterOrEqual");

    private async Task<PollingEventResponse<DateMemory, ListItemsResponse>> HandleItemsPolling(
        PollingEventRequest<DateMemory> request, string query)
    {
        if (request.Memory == null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var endpoint = $"/Search?{query}";
        var items = (await Client.Paginate<ItemEntity>(new SitecoreRequest(endpoint, Method.Get, Creds))).ToArray();

        if (items.Length == 0)
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };

        return new()
        {
            FlyBird = true,
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            },
            Result = new(items)
            {
            }
        };
    }
}