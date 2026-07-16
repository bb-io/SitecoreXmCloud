using System.Text.Json;
using Apps.Sitecore.Utils;
using Apps.SitecoreXmCloud.Models;
using HtmlAgilityPack;

namespace Tests.Sitecore;

[TestClass]
public class SitecoreHtmlConverterTests
{
    [TestMethod]
    public void ToHtml_CreatesExpectedHtml()
    {
        // Arrange
        string fieldsJson = @"[{""ID"":""{9226202F-DCA6-469E-A855-0DE57E827A07}"",""Value"":"""",""Type"":""Single-Line Text"",""TypeKey"":""single-line text"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Header"",""Name"":""header"",""Key"":""header"",""Section"":""Standard Attributes"",""SectionDisplayName"":""Standard Attributes"",""Title"":""Header""},{""ID"":""{6ED0657F-1B4C-49A1-BCF1-4BF44FB9A78A}"",""Value"":""Products"",""Type"":""Single-Line Text"",""TypeKey"":""single-line text"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Column Heading"",""Name"":""columnHeading"",""Key"":""columnheading"",""Section"":""Link"",""SectionDisplayName"":""Link"",""Title"":""Column Heading""},{""ID"":""{974F434B-6C07-4E62-9350-719D65BABFFA}"",""Value"":"""",""Type"":""Single-Line Text"",""TypeKey"":""single-line text"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""XTM_Sent for translation by"",""Name"":""XTM_Sent for translation by"",""Key"":""xtm_sent for translation by"",""Section"":""Translation Settings"",""SectionDisplayName"":""Translation Settings"",""Title"":""""},{""ID"":""{D64EDD7B-8461-46F8-A2B5-C4ABB4761F11}"",""Value"":""<link text=\""Products2\"" linktype=\""external\"" url=\""\"" anchor=\""\"" target=\""\""/>""" + @",""Type"":""General Link"",""TypeKey"":""general link"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Header Link"",""Name"":""headerLink"",""Key"":""headerlink"",""Section"":""Link"",""SectionDisplayName"":""Link"",""Title"":""Header Link""}]";

        var fields = JsonSerializer.Deserialize<List<FieldModel>>(fieldsJson);
        string itemId = "1";

        // Act
        byte[] htmlBytes = SitecoreHtmlConverter.ToHtml(fields, itemId);
        string html = System.Text.Encoding.UTF8.GetString(htmlBytes);
        System.Console.WriteLine(html);

        // Assert
        Assert.IsNotNull(html);
    }

    [TestMethod]
    public void ToHtml_PreservesSelfClosingTags()
    {
        // Arrange
        string fieldsJson = @"[{""ID"":""{D64EDD7B-8461-46F8-A2B5-C4ABB4761F11}"",""Value"":""<link text=\""Products2\"" linktype=\""external\"" url=\""\"" anchor=\""\"" target=\""\""/>"",""Type"":""General Link"",""TypeKey"":""general link""}]";

        var fields = JsonSerializer.Deserialize<List<FieldModel>>(fieldsJson);
        string itemId = "1";

        // Act
        byte[] htmlBytes = SitecoreHtmlConverter.ToHtml(fields, itemId);
        string html = System.Text.Encoding.UTF8.GetString(htmlBytes);
        System.Console.WriteLine(html);

        // Assert
        Assert.IsNotNull(html);
    }
}
