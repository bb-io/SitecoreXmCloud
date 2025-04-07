using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.SitecoreXmCloud.DataSourceHandlers.EnumHandlers;

    public class FileFormatDataHandler : IStaticDataSourceItemHandler
    {
        public IEnumerable<DataSourceItem> GetData() => new List<DataSourceItem>()
        {
            new DataSourceItem( "html", "HTML" ),
            new DataSourceItem( "json", "JSON" )
        };
    }
