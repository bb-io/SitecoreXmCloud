using System.Text;
using Apps.SitecoreXmCloud.Models;
using Blackbird.Applications.Sdk.Utils.Html.Extensions;
using HtmlAgilityPack;

namespace Apps.Sitecore.Utils;

public static class SitecoreHtmlConverter
{
    private const string IdAttr = "id";

    public static byte[] ToHtml(IEnumerable<FieldModel> fields, string itemId)
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

            fieldNode.SetAttributeValue(IdAttr, x.ID);
            if (!String.IsNullOrEmpty(x.Type))
            {
                fieldNode.SetAttributeValue("data-fieldType", x.Type);
            }
            if (!String.IsNullOrEmpty(x.Section))
            {
                fieldNode.SetAttributeValue("data-section", x.Section);
            }
            if (!String.IsNullOrEmpty(x.TypeKey))
            {
                fieldNode.SetAttributeValue("data-typeKey", x.TypeKey);
            }
            if (!String.IsNullOrEmpty(x.Name))
            {
                fieldNode.SetAttributeValue("data-name", x.Name);
            }
            if (!String.IsNullOrEmpty(x.DisplayName))
            {
                fieldNode.SetAttributeValue("data-displayName", x.DisplayName);
            }
            if (!String.IsNullOrEmpty(x.Key))
            {
                fieldNode.SetAttributeValue("data-key", x.Key);
            }
            if (!String.IsNullOrEmpty(x.SectionDisplayName))
            {
                fieldNode.SetAttributeValue("data-sectionDisplayName", x.SectionDisplayName);
            }
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