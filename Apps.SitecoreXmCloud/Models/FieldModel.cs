using System.Text.Json.Serialization;

namespace Apps.SitecoreXmCloud.Models;

public class FieldModel
{
    [JsonPropertyName("ID")]
    public string ID { get; set; }

    [JsonPropertyName("Value")]
    public string Value { get; set; }

    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("TypeKey")]
    public string? TypeKey { get; set; }

    [JsonPropertyName("Definition")]
    public string? Definition { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("DisplayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Key")]
    public string? Key { get; set; }

    [JsonPropertyName("Section")]
    public string? Section { get; set; }

    [JsonPropertyName("SectionDisplayName")]
    public string? SectionDisplayName { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}
