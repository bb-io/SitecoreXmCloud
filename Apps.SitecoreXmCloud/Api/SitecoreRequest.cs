using Apps.Sitecore.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using RestSharp;

namespace Apps.Sitecore.Api;

public class SitecoreRequest : BlackBirdRestRequest
{
    public SitecoreRequest(string endpoint, Method method,
        IEnumerable<AuthenticationCredentialsProvider> creds) : base(endpoint, method, creds)
    {
    }

    protected override void AddAuth(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        this.AddHeader("sc_apikey", creds.Get(CredsNames.ApiKey).Value);
    }
}