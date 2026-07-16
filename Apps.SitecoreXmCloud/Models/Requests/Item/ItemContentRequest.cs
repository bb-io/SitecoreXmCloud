using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Newtonsoft.Json;

namespace Apps.Sitecore.Models.Requests.Item;

public class ItemContentRequest : IDownloadContentInput
{
    [Display("Content ID")]
    [JsonProperty("itemId")]
    [DataSource(typeof(ItemDataHandler))]
    public string ContentId { get; set; } = string.Empty;

    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}