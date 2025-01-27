using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Apps.SitecoreXmCloud.Models.Responses;
public class OAuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}
