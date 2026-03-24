using Apps.SitecoreXmCloud.Models.Entities;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Newtonsoft.Json;

namespace Apps.SitecoreXmCloud.Polling.Response;

public class PollingItemEntity : BaseItemEntity, IDownloadContentInput
{
    [Display("Content ID"), JsonProperty("id")]
    public string ContentId { get; set; } = string.Empty;
}
