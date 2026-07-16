using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Apps.Sitecore.Utils;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Entities;

namespace Apps.SitecoreXmCloud.Utils;

public static class SitecoreJsonConverter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static byte[] ToJson(IEnumerable<FieldModel> fields, string itemId) =>
        ToJson(fields, new BlackbirdItemMetadata(itemId, null, null, string.Empty, string.Empty, null));

    public static byte[] ToJson(IEnumerable<FieldModel> fields, BlackbirdItemMetadata metadata)
    {
        var export = new JsonExportFormat
        {
            ItemID = metadata.ItemId,
            UCID = metadata.ItemId,
            Locale = metadata.Locale,
            ContentName = metadata.Name,
            AdminUrl = EmptyToNull(metadata.AdminUrl),
            SystemName = SitecoreHtmlConverter.SystemName,
            SystemRef = EmptyToNull(metadata.SystemRef),
            Fields = fields
        };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(export, SerializerOptions));
    }

    public static JsonObject Parse(byte[] json) =>
        JsonNode.Parse(json) is JsonObject root
            ? root
            : throw new JsonException("Expected a JSON object at the document root.");

    public static string? ExtractItemId(JsonObject root) =>
        (string?)root["blackbird-ucid"] ?? (string?)root["blackbird-item-id"];

    public static string? ExtractItemId(byte[] json) => ExtractItemId(Parse(json));

    public static Dictionary<string, string> ExtractFields(JsonObject root)
    {
        var result = new Dictionary<string, string>();
        if (root["fields"] is not JsonArray array)
        {
            return result;
        }

        foreach (var element in array)
        {
            if (element is not JsonObject field)
            {
                continue;
            }
            var id = (string?)field["id"];
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }
            result[id] = (string?)field["value"] ?? string.Empty;
        }
        return result;
    }

    public static byte[] WriteMetadata(JsonObject root, BlackbirdItemMetadata metadata)
    {
        root["blackbird-item-id"] = metadata.ItemId;
        root["blackbird-ucid"] = metadata.ItemId;
        SetIfPresent(root, "blackbird-locale", metadata.Locale);
        SetIfPresent(root, "blackbird-content-name", metadata.Name);
        SetIfPresent(root, "blackbird-admin-url", metadata.AdminUrl);
        root["blackbird-system-name"] = SitecoreHtmlConverter.SystemName;
        SetIfPresent(root, "blackbird-system-ref", metadata.SystemRef);
        return Encoding.UTF8.GetBytes(root.ToJsonString(SerializerOptions));
    }

    private static void SetIfPresent(JsonObject root, string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            root[key] = value;
        }
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrEmpty(value) ? null : value;

    private class JsonExportFormat
    {
        [JsonPropertyName("blackbird-item-id")]
        public string ItemID { get; set; } = string.Empty;

        [JsonPropertyName("blackbird-ucid")]
        public string UCID { get; set; } = string.Empty;

        [JsonPropertyName("blackbird-locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("blackbird-content-name")]
        public string? ContentName { get; set; }

        [JsonPropertyName("blackbird-admin-url")]
        public string? AdminUrl { get; set; }

        [JsonPropertyName("blackbird-system-name")]
        public string SystemName { get; set; } = string.Empty;

        [JsonPropertyName("blackbird-system-ref")]
        public string? SystemRef { get; set; }

        [JsonPropertyName("fields")]
        public IEnumerable<FieldModel> Fields { get; set; } = Array.Empty<FieldModel>();
    }
}
