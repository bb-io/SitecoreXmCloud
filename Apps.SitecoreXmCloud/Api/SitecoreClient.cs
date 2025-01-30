using Apps.Sitecore.Constants;
using Apps.Sitecore.Models.Responses;
using Apps.SitecoreXmCloud.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Applications.Sdk.Utils.Extensions.System;
using Blackbird.Applications.Sdk.Utils.RestSharp;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Sitecore.Api;

public class SitecoreClient : BlackBirdRestClient
{
    private const int PaginationStepSize = 20;
    
    public SitecoreClient(IEnumerable<AuthenticationCredentialsProvider> creds) :
        base(new()
        {
            BaseUrl = creds.Get(CredsNames.Url).Value.ToUri().Append("api/blackbird")
        })
    {
        this.AddDefaultHeader("sc_apikey", creds.Get(CredsNames.ApiKey).Value);

        var accessToken = GetBearerToken(creds);
        this.AddDefaultHeader("Authorization", $"Bearer {accessToken}");
    }

    private string GetBearerToken(IEnumerable<AuthenticationCredentialsProvider> creds)
    {
        var client = new RestClient("https://auth.sitecorecloud.io");
        var request = new RestRequest("/oauth/token", Method.Post);
        request.AddJsonBody(new
        {
            audience = "https://api.sitecorecloud.io",
            grant_type = "client_credentials",
            client_id = creds.Get(CredsNames.ClientId).Value,
            client_secret = creds.Get(CredsNames.ClientSecret).Value
        });

        try
        {
            var response = client.Post<OAuthResponse>(request);
            return response?.AccessToken;
        } catch(Exception ex)
        {
            if (ex.Message.Contains("Unauthorized"))
            {
                throw new PluginMisconfigurationException("Unauthorized. Please check your connection settings and reconnect the app.");
            }
            throw new PluginApplicationException(ex.Message, ex);
        }
    }

    public async Task<IEnumerable<T>> Paginate<T>(RestRequest request)
    {
        var page = 1;
        var baseUrl = request.Resource.SetQueryParameter("pageSize", PaginationStepSize.ToString());

        var result = new List<T>();
        T[] response;
        do
        {
            request.Resource = baseUrl.SetQueryParameter("page", page++.ToString());
            response = await ExecuteWithErrorHandling<T[]>(request);

            result.AddRange(response);
        } while (response.Any());

        return result;
    }

    protected override Exception ConfigureErrorException(RestResponse response)
    {
        var error = JsonConvert.DeserializeObject<ErrorResponse>(response.Content!)!;
        return new PluginApplicationException(error.Error);
    }
}