using System.Text.Json;
using Apps.Sitecore.Utils;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Entities;
using HtmlAgilityPack;

namespace Tests.Sitecore;

[TestClass]
public class SitecoreHtmlConverterTests
{
    private const string SampleFieldsJson = @"[{""ID"":""{9226202F-DCA6-469E-A855-0DE57E827A07}"",""Value"":"""",""Type"":""Single-Line Text"",""TypeKey"":""single-line text"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Header"",""Name"":""header"",""Key"":""header"",""Section"":""Standard Attributes"",""SectionDisplayName"":""Standard Attributes"",""Title"":""Header""},{""ID"":""{6ED0657F-1B4C-49A1-BCF1-4BF44FB9A78A}"",""Value"":""Products"",""Type"":""Single-Line Text"",""TypeKey"":""single-line text"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Column Heading"",""Name"":""columnHeading"",""Key"":""columnheading"",""Section"":""Link"",""SectionDisplayName"":""Link"",""Title"":""Column Heading""},{""ID"":""{D64EDD7B-8461-46F8-A2B5-C4ABB4761F11}"",""Value"":""<link text=\""Products2\"" linktype=\""external\"" url=\""\"" anchor=\""\"" target=\""\""/>"",""Type"":""General Link"",""TypeKey"":""general link"",""Definition"":""Sitecore.Data.Templates.TemplateField"",""Description"":"""",""DisplayName"":""Header Link"",""Name"":""headerLink"",""Key"":""headerlink"",""Section"":""Link"",""SectionDisplayName"":""Link"",""Title"":""Header Link""}]";

    private static readonly BlackbirdItemMetadata SampleMetadata = new(
        ItemId: "{ITEM-GUID-1}",
        Locale: "en",
        Name: "Sample Item",
        AdminUrl: "https://cm.example.com/sitecore/shell/Applications/Content%20Editor.aspx?fo=%7BITEM-GUID-1%7D&lang=en",
        SystemRef: "https://cm.example.com",
        UpdatedBy: "sitecore\\admin");

    [TestMethod]
    public void ToHtml_CreatesExpectedHtml()
    {
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(SampleFieldsJson);

        var htmlBytes = SitecoreHtmlConverter.ToHtml(fields!, "1");
        var html = System.Text.Encoding.UTF8.GetString(htmlBytes);

        Assert.IsNotNull(html);
    }

    [TestMethod]
    public void ToHtml_PreservesSelfClosingTags()
    {
        var fieldsJson = @"[{""ID"":""{D64EDD7B-8461-46F8-A2B5-C4ABB4761F11}"",""Value"":""<link text=\""Products2\"" linktype=\""external\"" url=\""\"" anchor=\""\"" target=\""\""/>"",""Type"":""General Link"",""TypeKey"":""general link""}]";
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(fieldsJson);

        var htmlBytes = SitecoreHtmlConverter.ToHtml(fields!, "1");
        var html = System.Text.Encoding.UTF8.GetString(htmlBytes);

        Assert.IsNotNull(html);
    }

    [TestMethod]
    public void ToHtml_IncludesRequiredBlackbirdMeta()
    {
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(SampleFieldsJson)!;

        var htmlBytes = SitecoreHtmlConverter.ToHtml(fields, SampleMetadata);
        var doc = new HtmlDocument();
        doc.LoadHtml(System.Text.Encoding.UTF8.GetString(htmlBytes));

        Assert.AreEqual("en", doc.DocumentNode.SelectSingleNode("/html").GetAttributeValue("lang", null));
        Assert.AreEqual(SampleMetadata.ItemId, MetaContent(doc, "blackbird-ucid"));
        Assert.AreEqual(SampleMetadata.Name, MetaContent(doc, "blackbird-content-name"));
        Assert.AreEqual(SampleMetadata.AdminUrl, MetaContent(doc, "blackbird-admin-url"));
        Assert.AreEqual("Sitecore XM Cloud", MetaContent(doc, "blackbird-system-name"));
        Assert.AreEqual(SampleMetadata.SystemRef, MetaContent(doc, "blackbird-system-ref"));
        Assert.AreEqual(SampleMetadata.ItemId, MetaContent(doc, "blackbird-item-id"));
    }

    [TestMethod]
    public void ToHtml_AddsItsRevAttributesOnBody()
    {
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(SampleFieldsJson)!;

        var htmlBytes = SitecoreHtmlConverter.ToHtml(fields, SampleMetadata);
        var doc = new HtmlDocument();
        doc.LoadHtml(System.Text.Encoding.UTF8.GetString(htmlBytes));
        var body = doc.DocumentNode.SelectSingleNode("/html/body");

        Assert.AreEqual("Sitecore XM Cloud", body.GetAttributeValue("its-rev-tool", null));
        Assert.AreEqual(SampleMetadata.SystemRef, body.GetAttributeValue("its-rev-tool-ref", null));
        Assert.AreEqual(SampleMetadata.UpdatedBy, body.GetAttributeValue("its-rev-person", null));
    }

    [TestMethod]
    public void ToHtml_AddsDataBlackbirdKeyPerField()
    {
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(SampleFieldsJson)!;

        var htmlBytes = SitecoreHtmlConverter.ToHtml(fields, SampleMetadata);
        var doc = new HtmlDocument();
        doc.LoadHtml(System.Text.Encoding.UTF8.GetString(htmlBytes));
        var fieldNodes = doc.DocumentNode.SelectNodes("/html/body/div");

        Assert.IsNotNull(fieldNodes);
        Assert.AreEqual(fields.Count, fieldNodes.Count);
        foreach (var node in fieldNodes)
        {
            var id = node.GetAttributeValue("id", null);
            Assert.AreEqual($"{SampleMetadata.ItemId}-{id}", node.GetAttributeValue("data-blackbird-key", null));
        }
    }

    [TestMethod]
    public void ExtractItemId_ReadsUcidFirst_FallsBackToItemId()
    {
        var withUcid = "<html><head><meta name=\"blackbird-ucid\" content=\"ucid-value\"/><meta name=\"blackbird-item-id\" content=\"legacy-value\"/></head><body></body></html>";
        var legacyOnly = "<html><head><meta name=\"blackbird-item-id\" content=\"legacy-value\"/></head><body></body></html>";

        Assert.AreEqual("ucid-value", SitecoreHtmlConverter.ExtractItemId(withUcid));
        Assert.AreEqual("legacy-value", SitecoreHtmlConverter.ExtractItemId(legacyOnly));
    }

    [TestMethod]
    public void WriteMetadata_RewritesMetaAndLangForTarget()
    {
        var fields = JsonSerializer.Deserialize<List<FieldModel>>(SampleFieldsJson)!;
        var sourceHtml = SitecoreHtmlConverter.ToHtml(fields, SampleMetadata);

        var target = new BlackbirdItemMetadata(
            ItemId: "{ITEM-GUID-1}",
            Locale: "fr",
            Name: "Article Exemple",
            AdminUrl: "https://cm.example.com/admin/fr",
            SystemRef: "https://cm.example.com",
            UpdatedBy: null);

        var parsed = SitecoreHtmlConverter.Parse(sourceHtml);
        var updatedBytes = SitecoreHtmlConverter.WriteMetadata(parsed, target);
        var doc = new HtmlDocument();
        doc.LoadHtml(System.Text.Encoding.UTF8.GetString(updatedBytes));

        Assert.AreEqual("fr", doc.DocumentNode.SelectSingleNode("/html").GetAttributeValue("lang", null));
        Assert.AreEqual(target.Name, MetaContent(doc, "blackbird-content-name"));
        Assert.AreEqual(target.AdminUrl, MetaContent(doc, "blackbird-admin-url"));
    }

    private static string? MetaContent(HtmlDocument doc, string name)
        => doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']")?.GetAttributeValue("content", null);
}
