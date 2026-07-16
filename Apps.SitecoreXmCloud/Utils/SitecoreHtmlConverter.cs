using System.Text;
using System.Text.RegularExpressions;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Entities;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Utils.Html.Extensions;
using HtmlAgilityPack;

namespace Apps.Sitecore.Utils;

public static class SitecoreHtmlConverter
{
    private const string IdAttr = "id";
    public const string SystemName = "Sitecore XM Cloud";
    private static readonly Regex OnlySelfClosingTagRegex =
        new(@"^\s*<([a-zA-Z][a-zA-Z0-9]*)[^>]*/\s*>\s*$", RegexOptions.Compiled);

    public static byte[] ToHtml(IEnumerable<FieldModel> fields, string itemId) =>
        ToHtml(fields, new BlackbirdItemMetadata(itemId, null, null, string.Empty, string.Empty, null));

    public static byte[] ToHtml(IEnumerable<FieldModel> fields, BlackbirdItemMetadata metadata)
    {
        var doc = new HtmlDocument();
        var htmlNode = doc.CreateElement("html");
        doc.DocumentNode.AppendChild(htmlNode);
        if (!string.IsNullOrEmpty(metadata.Locale))
        {
            htmlNode.SetAttributeValue("lang", metadata.Locale);
        }

        var headNode = doc.CreateElement("head");
        htmlNode.AppendChild(headNode);
        WriteMetadataToHead(doc, headNode, metadata);

        var bodyNode = doc.CreateElement("body");
        htmlNode.AppendChild(bodyNode);
        WriteItsRevisionAttributes(bodyNode, metadata);

        foreach (var field in fields)
        {
            bodyNode.AppendChild(BuildFieldNode(doc, field, metadata.ItemId));
        }

        return Encoding.UTF8.GetBytes(doc.DocumentNode.OuterHtml);
    }

    public static HtmlDocument Parse(byte[] html) =>
        Encoding.UTF8.GetString(html).AsHtmlDocument();

    public static string? ExtractItemId(HtmlDocument doc) =>
        GetMetaContent(doc, "blackbird-ucid") ?? GetMetaContent(doc, "blackbird-item-id");

    public static string? ExtractItemId(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return ExtractItemId(doc);
    }

    public static Dictionary<string, string> ExtractFields(HtmlDocument doc)
    {
        var bodyNode = doc.DocumentNode.SelectSingleNode("/html/body")
            ?? throw new PluginMisconfigurationException(
                "There is no content to extract from the provided file or the format was not the expected one.");

        try
        {
            return bodyNode.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Element)
                .ToDictionary(n => n.Attributes[IdAttr].Value, ReadFieldValue);
        }
        catch
        {
            throw new PluginMisconfigurationException(
                "There is no content to extract from the provided file or the format was not the expected one.");
        }
    }

    public static byte[] WriteMetadata(HtmlDocument doc, BlackbirdItemMetadata metadata)
    {
        var htmlNode = doc.DocumentNode.SelectSingleNode("/html") ?? doc.DocumentNode;
        var headNode = doc.DocumentNode.SelectSingleNode("/html/head");
        if (headNode == null)
        {
            headNode = doc.CreateElement("head");
            htmlNode.PrependChild(headNode);
        }

        if (!string.IsNullOrEmpty(metadata.Locale))
        {
            htmlNode.SetAttributeValue("lang", metadata.Locale);
        }
        WriteMetadataToHead(doc, headNode, metadata, replaceExisting: true);

        return Encoding.UTF8.GetBytes(doc.DocumentNode.OuterHtml);
    }

    private static void WriteMetadataToHead(HtmlDocument doc, HtmlNode headNode, BlackbirdItemMetadata metadata,
        bool replaceExisting = false)
    {
        SetMeta(doc, headNode, "blackbird-item-id", metadata.ItemId, replaceExisting);
        SetMeta(doc, headNode, "blackbird-ucid", metadata.ItemId, replaceExisting);
        SetMetaIfPresent(doc, headNode, "blackbird-content-name", metadata.Name, replaceExisting);
        SetMetaIfPresent(doc, headNode, "blackbird-admin-url", metadata.AdminUrl, replaceExisting);
        SetMeta(doc, headNode, "blackbird-system-name", SystemName, replaceExisting);
        SetMetaIfPresent(doc, headNode, "blackbird-system-ref", metadata.SystemRef, replaceExisting);
    }

    private static void WriteItsRevisionAttributes(HtmlNode bodyNode, BlackbirdItemMetadata metadata)
    {
        bodyNode.SetAttributeValue("its-rev-tool", SystemName);
        SetAttributeIfPresent(bodyNode, "its-rev-tool-ref", metadata.SystemRef);
        SetAttributeIfPresent(bodyNode, "its-rev-person", metadata.UpdatedBy);
    }

    private static HtmlNode BuildFieldNode(HtmlDocument doc, FieldModel field, string itemId)
    {
        var node = doc.CreateElement("div");
        node.SetAttributeValue(IdAttr, field.ID);
        if (!string.IsNullOrEmpty(field.ID))
        {
            node.SetAttributeValue("data-blackbird-key", $"{itemId}-{field.ID}");
        }

        SetAttributeIfPresent(node, "data-fieldType", field.Type);
        SetAttributeIfPresent(node, "data-section", field.Section);
        SetAttributeIfPresent(node, "data-typeKey", field.TypeKey);
        SetAttributeIfPresent(node, "data-name", field.Name);
        SetAttributeIfPresent(node, "data-displayName", field.DisplayName);
        SetAttributeIfPresent(node, "data-key", field.Key);
        SetAttributeIfPresent(node, "data-sectionDisplayName", field.SectionDisplayName);
        SetAttributeIfPresent(node, "data-description", field.Description);
        SetAttributeIfPresent(node, "data-definition", field.Definition);

        if (!string.IsNullOrEmpty(field.Value))
        {
            node.InnerHtml = field.Value;
            if (OnlySelfClosingTagRegex.IsMatch(field.Value))
            {
                node.SetAttributeValue("data-self-closing", "true");
            }
        }
        return node;
    }

    private static string ReadFieldValue(HtmlNode node)
    {
        if (!IsMarkedSelfClosing(node))
        {
            return node.InnerHtml;
        }

        var trimmed = node.InnerHtml.Trim();
        if (trimmed.EndsWith("/>"))
        {
            return trimmed;
        }

        var match = Regex.Match(trimmed, @"<([a-zA-Z][a-zA-Z0-9]*)([^>]*)>");
        return match.Success
            ? $"<{match.Groups[1].Value}{match.Groups[2].Value} />"
            : trimmed;
    }

    private static bool IsMarkedSelfClosing(HtmlNode node) =>
        node.Attributes["data-self-closing"]?.Value == "true";

    private static void SetMetaIfPresent(HtmlDocument doc, HtmlNode headNode, string name, string? value,
        bool replaceExisting)
    {
        if (!string.IsNullOrEmpty(value))
        {
            SetMeta(doc, headNode, name, value, replaceExisting);
        }
    }

    private static void SetMeta(HtmlDocument doc, HtmlNode headNode, string name, string value, bool replaceExisting)
    {
        if (replaceExisting)
        {
            var existing = headNode.SelectSingleNode($"meta[@name='{name}']");
            if (existing != null)
            {
                existing.SetAttributeValue("content", value);
                return;
            }
        }
        var meta = doc.CreateElement("meta");
        meta.SetAttributeValue("name", name);
        meta.SetAttributeValue("content", value);
        headNode.AppendChild(meta);
    }

    private static void SetAttributeIfPresent(HtmlNode node, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            node.SetAttributeValue(name, value);
        }
    }

    private static string? GetMetaContent(HtmlDocument doc, string name) =>
        doc.DocumentNode.SelectSingleNode($"//meta[@name='{name}']")?.GetAttributeValue("content", null);
}
