using System.Text;
using Blackbird.Applications.Sdk.Utils.Html.Extensions;
using HtmlAgilityPack;

namespace Apps.Sitecore.Utils;

public static class SitecoreHtmlConverter
{
    private const string IdAttr = "id";

    public static byte[] ToHtml(Dictionary<string, string> fields, string itemId)
    {
        var htmlDoc = new HtmlDocument();
        var htmlNode = htmlDoc.CreateElement("html");
        htmlDoc.DocumentNode.AppendChild(htmlNode);

        var headNode = htmlDoc.CreateElement("head");
        var metaNode = htmlDoc.CreateElement("meta");
        metaNode.SetAttributeValue("name", "blackbird-item-id");
        metaNode.SetAttributeValue("content", itemId);
        headNode.AppendChild(metaNode);
        htmlNode.AppendChild(headNode);

        var bodyNode = htmlDoc.CreateElement("body");
        htmlNode.AppendChild(bodyNode);

        fields.ToList().ForEach(x =>
        {
            var fieldNode = htmlDoc.CreateElement("div");

            fieldNode.SetAttributeValue(IdAttr, x.Key);
            fieldNode.InnerHtml = x.Value;

            bodyNode.AppendChild(fieldNode);
        });

        return Encoding.UTF8.GetBytes(htmlDoc.DocumentNode.OuterHtml);
    }

    public static Dictionary<string, string> ToSitecoreFields(byte[] html)
    {
        var htmlDoc = Encoding.UTF8.GetString(html).AsHtmlDocument();
        var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body");
        
        return bodyNode.ChildNodes.ToDictionary(x => x.Attributes[IdAttr].Value, x => x.InnerHtml);
    }
    
    public static string? ExtractItemIdFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-item-id']")?.GetAttributeValue("content", null);
    }
}