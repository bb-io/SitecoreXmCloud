using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;
using System.Globalization;

namespace Apps.Sitecore.Polling;

[PollingEventList]
public class PollingList(InvocationContext invocationContext) : SitecoreInvocable(invocationContext)
{
    [PollingEvent("On items created", "On new items created")]
    public Task<PollingEventResponse<DateMemory, ListItemsResponse>> OnItemsCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
    {
        var endpoint = $"/Search?locale={input.Locale}&rootPath={input.RootPath}";
        return HandleItemsCreatedPolling(request, endpoint);
    }

    [PollingEvent("On items updated", "On any items updated")]
    public Task<PollingEventResponse<DateMemory, ListItemsResponse>> OnItemsUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
    {
        var endpoint = $"/Search?locale={input.Locale}&rootPath={input.RootPath}";
        return HandleItemsUpdatedPolling(request, endpoint);
    }

    public async Task<PollingEventResponse<DateMemory, ListItemsResponse>> HandleItemsCreatedPolling(
        PollingEventRequest<DateMemory> request, string endpoint)
    {
        if (request.Memory?.LastInteractionDate != default(DateTime))
        {
            var createdAt = request.Memory.LastInteractionDate.ToString("o", CultureInfo.InvariantCulture);
            endpoint += $"&createdOperation=GreaterOrEqual&createdAt={Uri.EscapeDataString(createdAt)}";
        }
        
        var items = (await Client.Paginate<ItemEntity>(new SitecoreRequest(endpoint, Method.Get, Creds))).ToArray();

        if (items.Length == 0)
        {
            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = false,
                Memory = request.Memory ?? new DateMemory { LastInteractionDate = DateTime.UtcNow },
                Result = new ListItemsResponse(Array.Empty<ItemEntity>())
            };
        }

        if (request.Memory == null)
        {
            var maxCreatedAt = items.Max(i => i.CreatedAt);
            var memory = new DateMemory { LastInteractionDate = maxCreatedAt };
            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = false,
                Memory = memory,
                Result = new ListItemsResponse(items)
            };
        }

        var newItems = items.Where(i => i.CreatedAt > request.Memory.LastInteractionDate).ToArray();

        var newItemsMaxCreatedAt = newItems.Max(i => i.CreatedAt);
        request.Memory.LastInteractionDate = newItemsMaxCreatedAt;

        return new PollingEventResponse<DateMemory, ListItemsResponse>
        {
            FlyBird = newItems.Any(),
            Memory = request.Memory,
            Result = new ListItemsResponse(newItems)
        };
    }


    public async Task<PollingEventResponse<DateMemory, ListItemsResponse>> HandleItemsUpdatedPolling(
    PollingEventRequest<DateMemory> request, string endpoint)
    {

        var items = (await Client.Paginate<ItemEntity>(
            new SitecoreRequest(endpoint, Method.Get, Creds)
        )).ToArray();

        if (items.Length == 0)
        {
            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = false,
                Memory = request.Memory ?? new DateMemory { LastInteractionDate = DateTime.UtcNow }
            };
        }

        if (request.Memory == null)
        {
            var maxUpdatedAt = items.Max(i => i.UpdatedAt);
            var memory = new DateMemory { LastInteractionDate = maxUpdatedAt };
            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = false,
                Memory = memory
            };
        }

        var newItems = items.Where(i => i.UpdatedAt > request.Memory.LastInteractionDate).ToArray();

        if (newItems.Any())
        {
            var maxUpdatedAt = newItems.Max(i => i.UpdatedAt);
            request.Memory.LastInteractionDate = maxUpdatedAt;

            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = true,
                Memory = request.Memory,
                Result = new ListItemsResponse(newItems)
            };
        }
        else
        {
            return new PollingEventResponse<DateMemory, ListItemsResponse>
            {
                FlyBird = false,
                Memory = request.Memory
            };
        }
    }
}