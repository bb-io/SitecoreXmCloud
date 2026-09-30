using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Newtonsoft.Json;

namespace Apps.Sitecore.Models.Requests.Item;

public class ItemContentRequest : IDownloadContentInput
{
    private string _contentId = string.Empty;

    [Display("Content ID")]
    [JsonProperty("itemId")]
    [FileDataSource(typeof(ItemPickerDataSourceHandler))]
    public string ContentId
    {
        get => _contentId;
        set => _contentId = value?.Trim() ?? string.Empty;
    }

    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string? Locale { get; set; }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}