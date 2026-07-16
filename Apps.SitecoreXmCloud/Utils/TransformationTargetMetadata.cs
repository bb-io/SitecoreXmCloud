using Apps.Sitecore.Utils;
using Apps.SitecoreXmCloud.Models.Entities;
using Blackbird.Filters.Transformations;

namespace Apps.SitecoreXmCloud.Utils;

public static class TransformationTargetMetadata
{
    public static void ApplySitecoreTarget(Transformation transformation, BlackbirdItemMetadata metadata)
    {
        var reference = transformation.TargetSystemReference;
        reference.ContentId = metadata.ContentId;
        reference.SystemName = SitecoreHtmlConverter.SystemName;
        reference.SystemRef = metadata.SystemRef;
        reference.AdminUrl = metadata.AdminUrl;

        var contentName = metadata.ContentName ?? transformation.SourceSystemReference?.ContentName;
        if (!string.IsNullOrWhiteSpace(contentName))
        {
            reference.ContentName = contentName;
        }

        if (!string.IsNullOrEmpty(metadata.Locale))
        {
            transformation.TargetLanguage = metadata.Locale;
        }
    }
}
