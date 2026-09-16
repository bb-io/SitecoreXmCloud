using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models.Entities;
using Apps.SitecoreXmCloud.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using RestSharp;
using FileItem = Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File;

namespace Apps.Sitecore.DataSourceHandlers;

public class ItemPickerDataSourceHandler(InvocationContext invocationContext)
    : SitecoreInvocable(invocationContext), IAsyncFileDataSourceItemHandler
{
    public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context,
        CancellationToken cancellationToken)
    {
        var tree = await LoadTreeAsync();
        return tree.GetChildren(context?.FolderId).Select(ToFileDataItem).ToList();
    }

    public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context,
        CancellationToken cancellationToken)
    {
        var tree = await LoadTreeAsync();

        var breadcrumb = new List<FolderPathItem>
        {
            new() { Id = ItemTree.RootId, DisplayName = ItemTree.RootName }
        };
        breadcrumb.AddRange(tree.GetAncestors(context?.FileDataItemId)
            .Select(node => new FolderPathItem { Id = node.Id, DisplayName = node.Name }));

        return breadcrumb;
    }

    private async Task<ItemTree> LoadTreeAsync()
    {
        var request = new SitecoreRequest("/Search", Method.Get, Creds);
        var items = await Client.Paginate<ItemEntity>(request);

        return new ItemTree(items);
    }

    private static FileDataItem ToFileDataItem(ItemTreeNode node) => node.HasChildren
        ? new Folder { Id = node.Id, DisplayName = node.Name, Date = node.UpdatedAt, IsSelectable = true }
        : new FileItem { Id = node.Id, DisplayName = node.Name, Date = node.UpdatedAt, IsSelectable = true };
}
