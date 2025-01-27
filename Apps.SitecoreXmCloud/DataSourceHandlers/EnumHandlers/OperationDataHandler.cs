using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Utils.Sdk.DataSourceHandlers;

namespace Apps.Sitecore.DataSourceHandlers.EnumHandlers;

public class OperationDataHandler : IStaticDataSourceItemHandler
{
    public IEnumerable<DataSourceItem> GetData() => new List<DataSourceItem>()
    {
        new DataSourceItem( "GreaterOrEqual", "Greater or equal" ),
        new DataSourceItem( "LessOrEqual", "Less or equal" ),
        new DataSourceItem( "Equal", "Equal" ),
    };
}