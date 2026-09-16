using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Newtonsoft.Json;

namespace Apps.Sitecore.Models.Requests.Item;

public class ItemContentOptionalRequest
{
    [Display("Item ID")]
    [JsonProperty("itemId")]
    [FileDataSource(typeof(ItemPickerDataSourceHandler))]
    public string? ItemId { get; set; }

    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}