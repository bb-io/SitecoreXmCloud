using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Responses.Locale;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList]
public class LocalesActions : SitecoreInvocable
{
    public LocalesActions(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [Action("Get all configured languages", Description = "List all available languages")]
    public async Task<ListLocalesResponse> ListLocales()
    {
        var request = new SitecoreRequest("/Locales", Method.Get, Creds);

        var response = await Client.ExecuteWithErrorHandling<LocaleEntity[]>(request);
        return new(response);
    }
}