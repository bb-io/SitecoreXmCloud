using Apps.Sitecore.DataSourceHandlers;
using Apps.Sitecore.DataSourceHandlers.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Newtonsoft.Json;

namespace Apps.Sitecore.Models.Requests.Item;

public class SearchItemsRequest
{
    [Display("Root path")]
    [JsonProperty("rootPath")]
    public string? RootPath { get; set; }
    
    [Display("Created at")]
    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }
    
    [Display("Created operation")]
    [JsonProperty("createdOperation")]
    [StaticDataSource(typeof(OperationDataHandler))]
    public string? CreatedOperation { get; set; }
    
    [Display("Updated at")]
    [JsonProperty("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
    
    [Display("Updated operation")]
    [JsonProperty("updatedOperation")]
    [StaticDataSource(typeof(OperationDataHandler))]
    public string? UpdatedOperation { get; set; }

    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
    
    [Display("Is published")]
    [JsonProperty("isPublished")]
    public bool? IsPublished { get; set; }
}