using Blackbird.Applications.Sdk.Common;

namespace Apps.Sitecore.Models.Entities;

public class LocaleEntity
{
    [Display("Display name")] public string DisplayName { get; set; }

    [Display("Language")]
    public string Language { get; set; }

    [Display("Is primary")] public bool IsPrimary { get; set; }
}