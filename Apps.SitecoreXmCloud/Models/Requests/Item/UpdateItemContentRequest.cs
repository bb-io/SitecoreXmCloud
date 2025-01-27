using Blackbird.Applications.Sdk.Common;

namespace Apps.Sitecore.Models.Requests.Item;

public class UpdateItemContentRequest
{
    [Display("Add new version")]
    public bool? AddNewVersion { get; set; }
}