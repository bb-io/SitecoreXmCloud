using Apps.Sitecore.Models.Entities;
using Apps.SitecoreXmCloud.Utils;

namespace Tests.Sitecore;

[TestClass]
public class ItemTreeTests
{
    private const string HomeId = "{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}";
    private const string SampleId = "{45075B42-43A2-42C7-8E57-83DCE4B9DE88}";
    private const string SubItemId = "{DCE281D6-2FBB-4CA4-9AD9-AAFA6C3A2896}";
    private const string LeafId = "{CE1A6ABF-24CF-4BF7-9D11-4178FCE41059}";

    private static ItemTree CreateTree() => new(new[]
    {
        Item(HomeId, "Home", "/sitecore/content/home"),
        Item(SampleId, "Sample Item - 3", "/sitecore/content/home/sample item - 3"),
        Item(SubItemId, "Sub item", "/sitecore/content/home/sample item - 3/sub item"),
        Item(LeafId, "Dogs", "/sitecore/content/home/dogs")
    });

    private static ItemEntity Item(string id, string name, string fullPath, DateTime? updatedAt = null) => new()
    {
        Id = id,
        Name = name,
        FullPath = fullPath,
        UpdatedAt = updatedAt ?? new DateTime(2024, 1, 1)
    };

    [TestMethod]
    public void GetChildren_WithoutFolderId_ReturnsItemsDirectlyUnderContentRoot()
    {
        var children = CreateTree().GetChildren(null);

        CollectionAssert.AreEqual(new[] { HomeId }, children.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetChildren_WithRootId_ReturnsSameItemsAsEmptyFolderId()
    {
        var tree = CreateTree();

        CollectionAssert.AreEqual(
            tree.GetChildren(string.Empty).Select(x => x.Id).ToArray(),
            tree.GetChildren(ItemTree.RootId).Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetChildren_WithItemId_ReturnsDirectChildrenOnly()
    {
        var children = CreateTree().GetChildren(HomeId);

        CollectionAssert.AreEquivalent(new[] { LeafId, SampleId }, children.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetChildren_OrdersItemsByName()
    {
        var children = CreateTree().GetChildren(HomeId);

        CollectionAssert.AreEqual(new[] { "Dogs", "Sample Item - 3" }, children.Select(x => x.Name).ToArray());
    }

    [TestMethod]
    public void GetChildren_WithUnknownId_ReturnsContentRootChildren()
    {
        var children = CreateTree().GetChildren("{00000000-0000-0000-0000-000000000000}");

        CollectionAssert.AreEqual(new[] { HomeId }, children.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetChildren_MarksItemsWithChildrenAsContainers()
    {
        var children = CreateTree().GetChildren(HomeId);

        Assert.IsTrue(children.Single(x => x.Id == SampleId).HasChildren);
        Assert.IsFalse(children.Single(x => x.Id == LeafId).HasChildren);
    }

    [TestMethod]
    public void GetChildren_WithSeveralLanguageVersionsOfSameItem_ReturnsSingleNode()
    {
        var tree = new ItemTree(new[]
        {
            Item(HomeId, "Home", "/sitecore/content/home"),
            Item(LeafId, "Dogs", "/sitecore/content/home/dogs", new DateTime(2024, 1, 1)),
            Item(LeafId, "Dogs renamed", "/sitecore/content/home/dogs", new DateTime(2025, 1, 1))
        });

        var children = tree.GetChildren(HomeId);

        Assert.AreEqual(1, children.Count);
        Assert.AreEqual("Dogs renamed", children.Single().Name);
    }

    [TestMethod]
    public void GetChildren_IgnoresItemsWithoutIdentity()
    {
        var tree = new ItemTree(new[]
        {
            Item(HomeId, "Home", "/sitecore/content/home"),
            Item(string.Empty, "Broken", "/sitecore/content/broken"),
            Item("{00000000-0000-0000-0000-000000000001}", "No path", string.Empty)
        });

        CollectionAssert.AreEqual(new[] { HomeId }, tree.GetChildren(null).Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetAncestors_ForNestedItem_ReturnsParentsFromRootDownwards()
    {
        var ancestors = CreateTree().GetAncestors(SubItemId);

        CollectionAssert.AreEqual(new[] { HomeId, SampleId }, ancestors.Select(x => x.Id).ToArray());
    }

    [TestMethod]
    public void GetAncestors_ForItemDirectlyUnderContentRoot_ReturnsNothing()
    {
        Assert.AreEqual(0, CreateTree().GetAncestors(HomeId).Count);
    }

    [TestMethod]
    public void GetAncestors_ForUnknownId_ReturnsNothing()
    {
        Assert.AreEqual(0, CreateTree().GetAncestors("{00000000-0000-0000-0000-000000000000}").Count);
    }

    [TestMethod]
    public void GetAncestors_WithoutId_ReturnsNothing()
    {
        Assert.AreEqual(0, CreateTree().GetAncestors(null).Count);
    }

    [TestMethod]
    public void GetChildren_WithPathsInDifferentCasing_MatchesParentAndChild()
    {
        var tree = new ItemTree(new[]
        {
            Item(HomeId, "Home", "/Sitecore/Content/Home"),
            Item(LeafId, "Dogs", "/sitecore/content/home/dogs")
        });

        CollectionAssert.AreEqual(new[] { HomeId }, tree.GetChildren(null).Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { LeafId }, tree.GetChildren(HomeId).Select(x => x.Id).ToArray());
    }
}
