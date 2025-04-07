using System.Text.Json.Serialization;

namespace Apps.SitecoreXmCloud.Models
{
    public class FieldModel
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("type-key")]
        public string? TypeKey { get; set; }

        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("display-name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("section")]
        public string? Section { get; set; }

        [JsonPropertyName("section-display-name")]
        public string? SectionDisplayName { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
}
