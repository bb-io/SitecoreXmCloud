using Apps.Sitecore.Models.Entities;

namespace Apps.SitecoreXmCloud.Utils;

public class ItemTree
{
    public const string RootId = "v:root";
    public const string RootName = "Content";
    public const string RootPath = "/sitecore/content";

    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    private readonly Dictionary<string, ItemTreeNode> nodesById;
    private readonly Dictionary<string, ItemTreeNode> nodesByPath;
    private readonly ILookup<string, ItemTreeNode> nodesByParentPath;

    public ItemTree(IEnumerable<ItemEntity> items)
    {
        var latestItems = items
            .Where(x => !string.IsNullOrWhiteSpace(x.Id) && !string.IsNullOrWhiteSpace(x.FullPath))
            .GroupBy(x => x.Id, Comparer)
            .Select(group => group.MaxBy(x => x.UpdatedAt)!)
            .ToList();

        var parentPaths = latestItems
            .Select(x => GetParentPath(NormalizePath(x.FullPath)))
            .ToHashSet(Comparer);

        var nodes = latestItems
            .Select(x => ToNode(x, parentPaths))
            .ToList();

        nodesById = nodes
            .GroupBy(x => x.Id, Comparer)
            .ToDictionary(group => group.Key, group => group.First(), Comparer);
        nodesByPath = nodes
            .GroupBy(x => x.Path, Comparer)
            .ToDictionary(group => group.Key, group => group.First(), Comparer);
        nodesByParentPath = nodes.ToLookup(x => GetParentPath(x.Path), Comparer);
    }

    public IReadOnlyCollection<ItemTreeNode> GetChildren(string? folderId) => nodesByParentPath[ResolvePath(folderId)]
        .OrderBy(x => x.Name, Comparer)
        .ToList();

    public IReadOnlyCollection<ItemTreeNode> GetAncestors(string? itemId)
    {
        var ancestors = new Stack<ItemTreeNode>();

        if (itemId is null || !nodesById.TryGetValue(itemId, out var node))
        {
            return ancestors;
        }

        var parentPath = GetParentPath(node.Path);
        while (!IsRootPath(parentPath) && nodesByPath.TryGetValue(parentPath, out var parent))
        {
            ancestors.Push(parent);
            parentPath = GetParentPath(parent.Path);
        }

        return ancestors;
    }

    private string ResolvePath(string? folderId)
    {
        if (string.IsNullOrEmpty(folderId) || Comparer.Equals(folderId, RootId))
        {
            return RootPath;
        }

        return nodesById.TryGetValue(folderId, out var node) ? node.Path : RootPath;
    }

    private static ItemTreeNode ToNode(ItemEntity item, IReadOnlySet<string> parentPaths)
    {
        var path = NormalizePath(item.FullPath);
        return new ItemTreeNode(item.Id, item.Name, path, item.UpdatedAt, parentPaths.Contains(path));
    }

    private static bool IsRootPath(string path) => Comparer.Equals(path, RootPath) || string.IsNullOrEmpty(path);

    private static string NormalizePath(string path) => path.TrimEnd('/');

    private static string GetParentPath(string path)
    {
        var separatorIndex = path.LastIndexOf('/');
        return separatorIndex <= 0 ? string.Empty : path[..separatorIndex];
    }
}
