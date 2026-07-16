using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Polling.Memory;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using RestSharp;
using System.Globalization;
using Apps.Sitecore.Polling.Requests;
using Blackbird.Applications.SDK.Blueprints;
using Apps.SitecoreXmCloud.Polling.Response;

namespace Apps.Sitecore.Polling;

[PollingEventList("Content")]
public class ContentPollingList(InvocationContext invocationContext) : SitecoreInvocable(invocationContext)
{
    [PollingEvent("On content created", "Polls for items that have been created since the last poll.")]
    public Task<PollingEventResponse<DateMemory, ListItemsResponse>> OnItemsCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
    {
        var endpoint = $"/Search?locale={input.Locale}&rootPath={input.RootPath}";
        return HandleItemsCreatedPolling(request, endpoint);
    }

    [PollingEvent("On content updated", "Polls for items that have been updated since the last poll.")]
    [BlueprintEventDefinition(BlueprintEvent.ContentCreatedOrUpdatedMultiple)]
    public Task<PollingEventResponse<DateMemory, PollingItemsResponse>> OnItemsUpdated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input)
    {
        var endpoint = $"/Search?locale={input.Locale}&rootPath={input.RootPath}";
        return HandleItemsPolling(request, endpoint, true);
    }
    
    [PollingEvent("On content assigned to workflow state", "Polls for items that currently have a specific workflow state")]
    public Task<PollingEventResponse<DateMemory, PollingItemsResponse>> OnItemsWithWorkflowState(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] PollingItemRequest input,
        [PollingEventParameter] WorkflowStateRequest workflowStateRequest)
    {
        var endpoint = $"/Search?locale={input.Locale}&rootPath={input.RootPath}&currentStateId={workflowStateRequest.WorkflowStateId}";
        return HandleItemsPolling(request, endpoint, false);
    }

    private async Task<PollingEventResponse<DateMemory, ListItemsResponse>> HandleItemsCreatedPolling(
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


    public async Task<PollingEventResponse<DateMemory, PollingItemsResponse>> HandleItemsPolling(
        PollingEventRequest<DateMemory> request, 
        string endpoint, 
        bool filterForUpdatedDate)
    {
        var items = await Client.Paginate<PollingItemEntity>(new SitecoreRequest(endpoint, Method.Get, Creds));

        if (!items.Any())
        {
            return new PollingEventResponse<DateMemory, PollingItemsResponse>
            {
                FlyBird = false,
                Memory = request.Memory ?? new DateMemory { LastInteractionDate = DateTime.UtcNow }
            };
        }

        if (request.Memory == null)
        {
            var maxUpdatedAt = items.Max(i => i.UpdatedAt);
            var memory = new DateMemory { LastInteractionDate = maxUpdatedAt };
            return new PollingEventResponse<DateMemory, PollingItemsResponse>
            {
                FlyBird = false,
                Memory = memory
            };
        }
        
        
        var newItems = filterForUpdatedDate
            ? items.Where(i => i.UpdatedAt > request.Memory.LastInteractionDate).ToArray()
            : items;
        
        if (newItems.Any())
        {
            var maxUpdatedAt = newItems.Max(i => i.UpdatedAt);
            request.Memory.LastInteractionDate = maxUpdatedAt;

            return new PollingEventResponse<DateMemory, PollingItemsResponse>
            {
                FlyBird = true,
                Memory = request.Memory,
                Result = new PollingItemsResponse(newItems.ToList())
            };
        }
        else
        {
            return new PollingEventResponse<DateMemory, PollingItemsResponse>
            {
                FlyBird = false,
                Memory = request.Memory
            };
        }
    }
}