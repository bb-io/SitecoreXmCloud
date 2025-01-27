using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Newtonsoft.Json;

namespace Apps.Sitecore.Models.Requests.Item;

public class ItemContentOptionalRequest
{
    [Display("Item ID")]
    [JsonProperty("itemId")]
    [DataSource(typeof(ItemDataHandler))]
    public string? ItemId { get; set; }

    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}