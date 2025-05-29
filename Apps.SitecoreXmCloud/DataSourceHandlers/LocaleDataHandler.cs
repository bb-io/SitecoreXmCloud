using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.DataSourceHandlers;

public class LocaleDataHandler(InvocationContext invocationContext)
    : SitecoreInvocable(invocationContext), IAsyncDataSourceItemHandler
{
    public async Task<IEnumerable<DataSourceItem>> GetDataAsync(DataSourceContext context,
        CancellationToken cancellationToken)
    {
        var request = new SitecoreRequest("/Locales", Method.Get, Creds);
        var response = await Client.ExecuteWithErrorHandling<LocaleEntity[]>(request);

        return response
            .Where(x => context.SearchString is null ||
                        x.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(x => new DataSourceItem(x.Language, x.DisplayName.Split(':').First().Trim()));
    }
}