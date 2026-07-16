namespace Apps.SitecoreXmCloud.Models.Entities;

public record BlackbirdItemMetadata(
    string ContentId,
    string? Locale,
    string? ContentName,
    string AdminUrl,
    string SystemRef,
    string? UpdatedBy);
