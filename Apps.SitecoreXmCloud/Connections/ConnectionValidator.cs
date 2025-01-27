using Apps.Sitecore.Api;
using Apps.Sitecore.Models.Responses;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Newtonsoft.Json;
using RestSharp;

namespace Apps.Sitecore.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var creds = authProviders.ToArray();
            
            var client = new SitecoreClient(creds);
            var request = new SitecoreRequest("/Locales", Method.Get, creds);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessStatusCode) 
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<MessageResponse>(response.Content!)!;
                    return new()
                    {
                        IsValid = false,
                        Message = $"The Sitecore server returned the following message: {error.Message}. Please verify if the plugin was installed correctly."
                    };
                } catch (Exception ex)
                {
                    return new()
                    {
                        IsValid = false,
                        Message = response.StatusDescription
                    };
                }

            }

            return new()
            {
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsValid = false,
                Message = ex.Message
            };
        }
    }
}