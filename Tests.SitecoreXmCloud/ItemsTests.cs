using Apps.Sitecore.Actions;
using Apps.Sitecore.Models;
using Apps.Sitecore.Models.Requests.Item;
using Apps.SitecoreXmCloud.Models.Requests.Item;
using Blackbird.Applications.Sdk.Common.Files;
using Tests.Sitecore.Base;

namespace Tests.Sitecore;

[TestClass]
public class ItemsTests : TestBase
{
    [TestMethod]
    public async Task Search_Items_works()
    {
        var actions = new ItemsActions(InvocationContext);

        var result = await actions.SearchItems(new SearchItemsRequest
        {
            RootPath = "/sitecore/content/GoTo/LogMeIn/Home",

            Locale = "en",
        });
        Console.WriteLine($"Total items: {result.Items.Count()}");

        foreach (var item in result.Items)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine($"Item ID: {item.Id}");
            Console.WriteLine($"Name: {item.Name}");
            Console.WriteLine($"Language: {item.Language}");
            Console.WriteLine($"Version: {item.Version}");
            Console.WriteLine($"Full path: {item.FullPath}");
            Console.WriteLine($"Created at: {item.CreatedAt}");
            Console.WriteLine($"Updated at: {item.UpdatedAt}");
            Console.WriteLine("=====================================");
        }

        Assert.IsTrue(result.Items.Count() > 0);
    }


    [TestMethod]
    public async Task Get_Item_As_HTML_works()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var input = new ItemContentRequest
        {
            ItemId = "1",
            //Version = "Copy of About Us_1",
            Locale = "en",
        };

        var fileFormat = new FileFormatInput { Format = "html" };
        var filter = new FilteringOptions
        {
            Type =
            [
                "Rich Text",
                "Checkbox",
                "Date",
                "Datetime",
                "Droptree",
                "File",
                "Hidden",
                "Integer",
                "Multilist",
                "Number",
                "Treelist",
                "TreelistEx",
                "Droplink"
            ],
            Description = ["DNT"]
        };

        var result = await actions.GetItemContent(input, fileFormat, filter);

        Console.WriteLine($"File: {result.File.Name}");
        Assert.IsNotNull(result.File);
    }

    [TestMethod]
    public async Task Update_Item_from_HTML_works()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var input = new ItemContentOptionalRequest
        {
            ItemId = "{909F3372-462D-4808-BD78-DCF6A979AEC3}"
        };

        var file = new FileModel
        {
            File = new FileReference
            {
                Name = "{909F3372-462D-4808-BD78-DCF6A979AEC3}.html"
            }
        };
        var item = new UpdateItemContentRequest
        {
        };

        await actions.UpdateItemContent(input, file, item);
        Assert.IsTrue(true);
    }
}