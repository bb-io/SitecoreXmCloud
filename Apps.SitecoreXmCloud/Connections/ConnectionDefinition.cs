using Apps.Sitecore.Constants;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;

namespace Apps.Sitecore.Connections;

public class ConnectionDefinition : IConnectionDefinition
{
    public IEnumerable<ConnectionPropertyGroup> ConnectionPropertyGroups => new List<ConnectionPropertyGroup>()
    {
        new()
        {
            Name = "ApiToken",
            AuthenticationType = ConnectionAuthenticationType.Undefined,
            ConnectionProperties = new List<ConnectionProperty>()
            {
                new(CredsNames.Url) { DisplayName = "Instance URL" },
                new(CredsNames.ApiKey) { DisplayName = "API key", Sensitive = true },
                new(CredsNames.ClientId) { DisplayName = "Client ID", Sensitive = false },
                new(CredsNames.ClientSecret) { DisplayName = "Client secret", Sensitive = true },
            }
        }
    };

    public IEnumerable<AuthenticationCredentialsProvider> CreateAuthorizationCredentialsProviders(
        Dictionary<string, string> values)
    {
        yield return new AuthenticationCredentialsProvider(
            CredsNames.Url,
            values[CredsNames.Url]
        );

        yield return new AuthenticationCredentialsProvider(
            CredsNames.ApiKey,
            values[CredsNames.ApiKey]
        );

        yield return new AuthenticationCredentialsProvider(
            CredsNames.ClientId,
            values[CredsNames.ClientId]
        );

        yield return new AuthenticationCredentialsProvider(
            CredsNames.ClientSecret,
            values[CredsNames.ClientSecret]
        );
    }
}