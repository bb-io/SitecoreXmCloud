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
            var response = await ExecuteValidationRequest(creds);

            if (!response.IsSuccessStatusCode)
            {
                return HandleErrorResponse(response);
            }

            if(response.ContentType == "text/html")
            {
                return new()
                {
                    IsValid = false,
                    Message = "The Sitecore server returned an HTML response. Please verify the URL you provided, our plugin installed correctly, and ensure that the server is accessible without firewall restrictions."
                };
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
    
    private async Task<RestResponse> ExecuteValidationRequest(AuthenticationCredentialsProvider[] creds)
    {
        var client = new SitecoreClient(creds);
        var request = new SitecoreRequest("/Locales", Method.Get, creds);
        return await client.ExecuteAsync(request);
    }
    
    private ConnectionValidationResponse HandleErrorResponse(RestResponse response)
    {
        try
        {
            if(string.IsNullOrEmpty(response.Content))
            {
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    return new()
                    {
                        IsValid = false,
                        Message = "The Sitecore server returned an empty response. Please verify the URL you provided, and ensure it accessible and doesn't have firewall restrictions."
                    };
                }
                
                return new()
                {
                    IsValid = false,
                    Message = response.ErrorMessage
                };
            }
            
            var error = JsonConvert.DeserializeObject<MessageResponse>(response.Content!)!;
            return new()
            {
                IsValid = false,
                Message = $"The Sitecore server returned the following message: {error.Message}. Please verify if the plugin was installed correctly."
            };
        } 
        catch (Exception)
        {
            return new()
            {
                IsValid = false,
                Message = response.ErrorMessage
            };
        }
    }
}