using Apps.Sitecore.DataSourceHandlers;
using Apps.SitecoreXmCloud.Utils;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Tests.Sitecore.Base;

namespace Tests.Sitecore
{
    [TestClass]
    public class DataHandlerTests : TestBase
    {
        [TestMethod]
        public async Task ItemPickerGetFolderContent_WithoutFolderId_ReturnsContentRootItems()
        {
            var handler = new ItemPickerDataSourceHandler(InvocationContext);

            var result = await handler.GetFolderContentAsync(new FolderContentDataSourceContext(), CancellationToken.None);

            var items = result.ToList();
            foreach (var item in items)
            {
                Console.WriteLine($"{item.Id} - {item.DisplayName} ({(item is Folder ? "folder" : "file")})");
            }

            Assert.IsTrue(items.Count > 0);
            Assert.IsTrue(items.All(x => x.IsSelectable));
        }

        [TestMethod]
        public async Task ItemPickerGetFolderContent_WithFolderId_ReturnsChildrenOfThatItem()
        {
            var handler = new ItemPickerDataSourceHandler(InvocationContext);
            var root = await handler.GetFolderContentAsync(new FolderContentDataSourceContext(), CancellationToken.None);
            var container = root.OfType<Folder>().First();

            var result = await handler.GetFolderContentAsync(
                new FolderContentDataSourceContext { FolderId = container.Id }, CancellationToken.None);

            var items = result.ToList();
            foreach (var item in items)
            {
                Console.WriteLine($"{item.Id} - {item.DisplayName}");
            }

            Assert.IsTrue(items.Count > 0);
        }

        [TestMethod]
        public async Task ItemPickerGetFolderPath_ForNestedItem_StartsAtContentRoot()
        {
            var handler = new ItemPickerDataSourceHandler(InvocationContext);
            var root = await handler.GetFolderContentAsync(new FolderContentDataSourceContext(), CancellationToken.None);
            var container = root.OfType<Folder>().First();
            var child = (await handler.GetFolderContentAsync(
                new FolderContentDataSourceContext { FolderId = container.Id }, CancellationToken.None)).First();

            var result = await handler.GetFolderPathAsync(
                new FolderPathDataSourceContext { FileDataItemId = child.Id }, CancellationToken.None);

            var breadcrumb = result.ToList();
            foreach (var item in breadcrumb)
            {
                Console.WriteLine($"{item.Id} - {item.DisplayName}");
            }

            Assert.AreEqual(ItemTree.RootId, breadcrumb.First().Id);
            Assert.AreEqual(container.Id, breadcrumb.Last().Id);
        }
    }
}
