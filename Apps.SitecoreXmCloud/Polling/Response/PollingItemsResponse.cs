using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.SitecoreXmCloud.Polling.Response;

public record PollingItemsResponse(List<PollingItemEntity> Items) : IMultiDownloadableContentOutput<PollingItemEntity>
{
    public List<PollingItemEntity> Items { get; set; } = Items;
}
