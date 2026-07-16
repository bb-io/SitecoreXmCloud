using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common;
using Apps.SitecoreXmCloud.Models.Entities;

namespace Apps.Sitecore.Models.Entities;

public class ItemEntity : BaseItemEntity, IContentOutput
{
    [Display("Content ID")]
    public string Id { get; set; } = string.Empty;
}