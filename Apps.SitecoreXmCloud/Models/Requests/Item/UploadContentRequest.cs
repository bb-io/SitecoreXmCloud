using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Blackbird.Applications.Sdk.Common.Files;
using Newtonsoft.Json;

namespace Apps.SitecoreXmCloud.Models.Requests.Item;

public class UploadContentRequest : IUploadContentInput
{
    private string? _contentId;

    public FileReference Content { get; set; } = null!;
    
    [Display("Language")]
    [JsonProperty("locale")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; } = string.Empty;
    
    [Display("Item ID")]
    [JsonProperty("itemId")]
    [FileDataSource(typeof(ItemPickerDataSourceHandler))]
    public string? ContentId
    {
        get => _contentId;
        set => _contentId = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
    
    [JsonProperty("version")]
    public string? Version { get; set; }
}