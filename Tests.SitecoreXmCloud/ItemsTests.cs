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
}
