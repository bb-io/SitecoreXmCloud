using Apps.SitecoreXmCloud.DataSourceHandlers.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.SitecoreXmCloud.Models.Requests.Item
{
    public class FileFormatInput
    {
        [Display("File format")]
        [StaticDataSource(typeof(FileFormatDataHandler))]
        public string Format { get; set; }
    }
}
