using Apps.Sitecore.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Sitecore.Polling;

public class PollingItemRequest
{
    [Display("Language")]
    [DataSource(typeof(LocaleDataHandler))]
    public string Locale { get; set; }
}