using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;

namespace Apps.SitecoreXmCloud.Models.Requests.Item;

public class UploadContentRequest : IUploadContentInput
{
    public FileReference Content { get; set; } = null!;
    
    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
    
    [Display("Item ID")]
    [JsonProperty("itemId")]
    [DataSource(typeof(ItemDataHandler))]
    public string? ContentId { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}