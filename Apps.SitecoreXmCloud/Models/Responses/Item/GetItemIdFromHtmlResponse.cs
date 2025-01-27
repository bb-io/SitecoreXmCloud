using Blackbird.Applications.Sdk.Common;

namespace Apps.Sitecore.Models.Responses.Item;

public class GetItemIdFromHtmlResponse
{
    [Display("Item ID")]
    public string ItemId { get; set; } = string.Empty;
}