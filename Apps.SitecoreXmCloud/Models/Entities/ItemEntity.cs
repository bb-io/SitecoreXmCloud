using Blackbird.Applications.Sdk.Common;

namespace Apps.Sitecore.Models.Entities;

public class ItemEntity
{
    [Display("Item ID")]
    public string Id { get; set; }
    
    public string Name { get; set; }
    
    public string Language { get; set; }
    
    public string Version { get; set; }
    
    [Display("Full path")]
    public string FullPath { get; set; }
    
    [Display("Created at")]
    public DateTime CreatedAt { get; set; }
    
    [Display("Updated at")]
    public DateTime UpdatedAt { get; set; }
}