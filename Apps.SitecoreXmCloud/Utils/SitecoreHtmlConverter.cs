using System.Text;
using System.Text.RegularExpressions;
using Apps.SitecoreXmCloud.Models;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Html.Extensions;
using HtmlAgilityPack;

namespace Apps.Sitecore.Utils;

public static class SitecoreHtmlConverter
{
    private const string IdAttr = "id";
    private static readonly Regex OnlySelfClosingTagRegex = new(@"^\s*<([a-zA-Z][a-zA-Z0-9]*)[^>]*/\s*>\s*$", RegexOptions.Compiled);

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
            if (!String.IsNullOrEmpty(x.Description))
            {
                fieldNode.SetAttributeValue("data-description", x.Description);
            }
            if (!String.IsNullOrEmpty(x.Definition))
            {
                fieldNode.SetAttributeValue("data-definition", x.Definition);
            }

            if (!string.IsNullOrEmpty(x.Value))
            {
                fieldNode.InnerHtml = x.Value;
                if (OnlySelfClosingTagRegex.IsMatch(x.Value))
                {
                    fieldNode.SetAttributeValue("data-self-closing", "true");
                }
            }

            bodyNode.AppendChild(fieldNode);
        });

        string html = htmlDoc.DocumentNode.OuterHtml;
        return Encoding.UTF8.GetBytes(html);
    }

    public static Dictionary<string, string> ToSitecoreFields(byte[] html)
    {
        var htmlDoc = Encoding.UTF8.GetString(html).AsHtmlDocument();
        var bodyNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body");
        try
        {
            return bodyNode.ChildNodes.ToDictionary(x => x.Attributes[IdAttr].Value, x =>
            {
                if (x.Attributes.Contains("data-self-closing") && x.Attributes["data-self-closing"].Value == "true")
                {
                    string innerHtml = x.InnerHtml.Trim();
                    if (innerHtml.EndsWith("/>"))
                    {
                        return innerHtml;
                    }
                    else
                    {
                        var tagMatch = Regex.Match(innerHtml, @"<([a-zA-Z][a-zA-Z0-9]*)([^>]*)>");
                        if (tagMatch.Success)
                        {
                            string tagName = tagMatch.Groups[1].Value;
                            string attributes = tagMatch.Groups[2].Value;
                            return $"<{tagName}{attributes} />";
                        }
                        return innerHtml;
                    }
                }

                return x.InnerHtml;
            });
        }
        catch
        {
            throw new PluginMisconfigurationException("There is no content to extract from the provided file or the format was not the expected one.");
        }
    }

    public static string? ExtractItemIdFromHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc.DocumentNode.SelectSingleNode("//meta[@name='blackbird-item-id']")?.GetAttributeValue("content", null);
    }
}