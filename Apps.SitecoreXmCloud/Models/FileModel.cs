using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.Sitecore.Models;

public class FileModel : IDownloadContentOutput
{
    public FileReference Content { get; set; }
}