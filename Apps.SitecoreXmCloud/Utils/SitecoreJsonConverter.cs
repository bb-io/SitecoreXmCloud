using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Apps.SitecoreXmCloud.Models;
using System.Text.Encodings.Web;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.SitecoreXmCloud.Utils
{
    public class SitecoreJsonConverter
    {
        public static byte[] GetJsonBytes(IEnumerable<FieldModel> fields, string itemId)
        {
            var exportObject = new JsonExportFormat
            {
                ItemID = itemId,
                Fields = fields
            };

            string jsonString = JsonSerializer.Serialize(exportObject, new JsonSerializerOptions {WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static async Task<Dictionary<string, string>> ExtractFromJsonAsync(byte[] jsonBytes)
        {
            var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;

            var dictionary = new Dictionary<string, string>();

            if (root.TryGetProperty("fields", out JsonElement fieldArray) && fieldArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in fieldArray.EnumerateArray())
                {
                    if (item.TryGetProperty("id", out var idProp) &&
                        item.TryGetProperty("value", out var valueProp))
                    {
                        string id = idProp.GetString();
                        string value = valueProp.GetString();

                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            dictionary[id] = value ?? string.Empty;
                        }
                    }
                }
            }

            return (dictionary);
        }
    
        public static async Task<string> ExtractItemIdFromJson(byte[] jsonBytes)
        {
            var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;

            if (root.TryGetProperty("blackbird-item-id", out JsonElement ItemID))
            {
                return ItemID.GetString();
            }
            
            throw new PluginMisconfigurationException("Blackbird item ID (blackbird-item-id) was not found in the provided file");
            
        }

    public class JsonExportFormat
        {
            [JsonPropertyName("blackbird-item-id")]
            public string ItemID { get; set; }

            [JsonPropertyName("fields")]
            public IEnumerable<FieldModel> Fields { get; set; }
        }
    }
}
