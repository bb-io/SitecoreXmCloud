using Apps.Sitecore.Actions;
using Apps.Sitecore.Connections;
using Apps.Sitecore.Models.Requests.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            CreatedAt = DateTime.Now.AddDays(-300),
            CreatedOperation = "GreaterOrEqual",
            Locale = "en",
        });
        Console.WriteLine($"Total items: {result.Items.Count()}");
        Console.WriteLine(JsonConvert.SerializeObject(result.Items, Formatting.Indented));
        Assert.IsTrue(result.Items.Count() > 0);
    }


    [TestMethod]
    public async Task Get_Item_As_HTML_works()
    {
        var actions = new ContentActions(InvocationContext, FileManager);
        var input = new ItemContentRequest { 
            ItemId = "{6E6E9C8F-2D14-4B67-81EF-0770715C4C41}",
            Version = "Copy of About Us_1"
        };

        var result = await actions.GetItemContent(input);

        Console.WriteLine($"File: {result.File.Name}");
        Assert.IsNotNull(result.File);
    }
}
