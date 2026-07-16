namespace Apps.SitecoreXmCloud.Models.Entities;

public record BlackbirdItemMetadata(
    string ItemId,
    string? Locale,
    string? Name,
    string AdminUrl,
    string SystemRef,
    string? UpdatedBy);
