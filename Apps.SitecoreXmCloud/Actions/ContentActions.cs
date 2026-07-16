using System.Net.Mime;
using System.Web;
using Apps.Sitecore.Api;
using Apps.Sitecore.Constants;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Requests.Item;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Entities;
using Apps.SitecoreXmCloud.Models.Requests.Item;
using Apps.SitecoreXmCloud.Utils;

namespace Apps.Sitecore.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : SitecoreInvocable(invocationContext)
{
    private const string HtmlExtension = ".html";
    private const string JsonExtension = ".json";

    [Action("Download item content", Description = "Get content of the specific item in a file")]
    public async Task<FileModel> GetItemContent(
        [ActionParameter] ItemContentRequest input,
        [ActionParameter] FileFormatInput format,
        [ActionParameter] FilteringOptions filter)
    {
        var fields = ApplyFilters(await FetchItemFields(input), filter);
        var metadata = await BuildItemMetadataAsync(input.ItemId, input.Locale);

        return format.Format switch
        {
            "html" => await UploadFile(SitecoreHtmlConverter.ToHtml(fields, metadata),
                MediaTypeNames.Text.Html, $"{input.ItemId}.html"),
            "json" => await UploadFile(SitecoreJsonConverter.ToJson(fields, metadata),
                MediaTypeNames.Application.Json, $"{input.ItemId}.json"),
            _ => new FileModel()
        };
    }

    [Action("Upload item content", Description = "Update content of the specific item from a file")]
    public async Task<FileModel> UpdateItemContent(
        [ActionParameter] ItemContentOptionalRequest itemContent,
        [ActionParameter] FileModel file,
        [ActionParameter] UpdateItemContentRequest input)
    {
        var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var fileBytes = await fileStream.GetByteData();

        var parsed = ParseUploadedFile(file.File.Name, fileBytes);

        itemContent.ItemId = itemContent.ItemId ?? parsed.ItemId
            ?? throw new PluginMisconfigurationException(
                "Didn't find item Item ID in the HTML file. Please provide it in the input.");

        if (input.AddNewVersion is true)
        {
            itemContent.Version = null;
            await CreateItemContent(new ItemContentRequest
            {
                ItemId = itemContent.ItemId,
                Version = itemContent.Version,
                Locale = itemContent.Locale
            });
        }

        var request = new SitecoreRequest("/Content".WithQuery(itemContent), Method.Put, Creds);
        foreach (var pair in parsed.Fields)
        {
            request.AddParameter($"fields[{pair.Key}]",
                HttpUtility.UrlEncode(HttpUtility.UrlEncode(pair.Value)));
        }
        await Client.ExecuteWithErrorHandling(request);

        var metadata = await BuildItemMetadataAsync(itemContent.ItemId, itemContent.Locale);
        var updatedBytes = parsed.WriteMetadata(metadata);
        return await UploadFile(updatedBytes, parsed.MediaType, file.File.Name);
    }

    [Action("Get Item ID from file", Description = "Extract Item ID from file")]
    public async Task<GetItemIdFromHtmlResponse> GetItemIdFromHtml([ActionParameter] FileModel file)
    {
        var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var fileBytes = await fileStream.GetByteData();

        var itemId = file.File.Name switch
        {
            var n when n.EndsWith(HtmlExtension, StringComparison.OrdinalIgnoreCase) =>
                SitecoreHtmlConverter.ExtractItemId(SitecoreHtmlConverter.Parse(fileBytes)),
            var n when n.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase) =>
                SitecoreJsonConverter.ExtractItemId(fileBytes),
            _ => null
        };

        return new GetItemIdFromHtmlResponse { ItemId = itemId ?? string.Empty };
    }

    [Action("Delete item content", Description = "Delete specific version of item's content")]
    public Task DeleteItemContent([ActionParameter] ItemContentRequest input)
    {
        var request = new SitecoreRequest("/Content".WithQuery(input), Method.Delete, Creds);
        return Client.ExecuteWithErrorHandling(request);
    }

    private Task CreateItemContent(ItemContentRequest input)
    {
        var request = new SitecoreRequest("/Content".WithQuery(input), Method.Post, Creds);
        return Client.ExecuteWithErrorHandling(request);
    }

    private async Task<List<FieldModel>> FetchItemFields(ItemContentRequest input)
    {
        var request = new SitecoreRequest("/Content".WithQuery(input), Method.Get, Creds);
        try
        {
            return await Client.ExecuteWithErrorHandling<List<FieldModel>>(request);
        }
        catch
        {
            var fallback = await Client.ExecuteWithErrorHandling<Dictionary<string, string>>(request);
            return fallback.Select(x => new FieldModel { ID = x.Key, Value = x.Value }).ToList();
        }
    }

    private static List<FieldModel> ApplyFilters(List<FieldModel> fields, FilteringOptions filter)
    {
        IEnumerable<FieldModel> query = fields;
        if (filter.Section != null) query = query.Where(x => !filter.Section.Contains(x.Section));
        if (filter.Key != null) query = query.Where(x => !filter.Key.Contains(x.Key));
        if (filter.TypeKey != null) query = query.Where(x => !filter.TypeKey.Contains(x.TypeKey));
        if (filter.Name != null) query = query.Where(x => !filter.Name.Contains(x.Name));
        if (filter.Type != null) query = query.Where(x => !filter.Type.Contains(x.Type));
        if (filter.DisplayName != null) query = query.Where(x => !filter.DisplayName.Contains(x.DisplayName));
        if (filter.Value != null) query = query.Where(x => !filter.Value.Contains(x.Value));
        if (filter.Title != null) query = query.Where(x => !filter.Title.Contains(x.Title));
        if (filter.Description != null) query = query.Where(x => !filter.Description.Contains(x.Description));
        if (filter.Definition != null) query = query.Where(x => !filter.Definition.Contains(x.Definition));
        if (filter.SectionDisplayName != null) query = query.Where(x => !filter.SectionDisplayName.Contains(x.SectionDisplayName));
        return query.ToList();
    }

    private async Task<FileModel> UploadFile(byte[] bytes, string mediaType, string fileName)
    {
        var uploaded = await fileManagementClient.UploadAsync(new MemoryStream(bytes), mediaType, fileName);
        return new FileModel { File = uploaded };
    }

    private static ParsedUpload ParseUploadedFile(string fileName, byte[] bytes)
    {
        if (fileName.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase))
        {
            var root = SitecoreJsonConverter.Parse(bytes);
            return new ParsedUpload(
                SitecoreJsonConverter.ExtractItemId(root),
                SitecoreJsonConverter.ExtractFields(root),
                MediaTypeNames.Application.Json,
                metadata => SitecoreJsonConverter.WriteMetadata(root, metadata));
        }

        if (fileName.EndsWith(HtmlExtension, StringComparison.OrdinalIgnoreCase))
        {
            var doc = SitecoreHtmlConverter.Parse(bytes);
            return new ParsedUpload(
                SitecoreHtmlConverter.ExtractItemId(doc),
                SitecoreHtmlConverter.ExtractFields(doc),
                MediaTypeNames.Text.Html,
                metadata => SitecoreHtmlConverter.WriteMetadata(doc, metadata));
        }

        throw new PluginMisconfigurationException(
            "Unsupported file format. Expected an .html or .json file.");
    }

    private async Task<BlackbirdItemMetadata> BuildItemMetadataAsync(string itemId, string? locale)
    {
        var instanceUrl = Creds.Get(CredsNames.Url).Value.TrimEnd('/');
        var adminUrl = BuildContentEditorUrl(instanceUrl, itemId, locale);
        var item = await TryGetItemAsync(itemId, locale);
        return new BlackbirdItemMetadata(itemId, locale, item?.Name, adminUrl, instanceUrl, item?.UpdatedBy);
    }

    private async Task<ItemEntity?> TryGetItemAsync(string itemId, string? locale)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return null;
        }

        try
        {
            var endpoint = string.IsNullOrEmpty(locale)
                ? "/Search"
                : $"/Search?locale={Uri.EscapeDataString(locale)}";
            var items = await Client.Paginate<ItemEntity>(new SitecoreRequest(endpoint, Method.Get, Creds));
            return items
                .Where(x => string.Equals(x.Id, itemId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => int.TryParse(x.Version, out var v) ? v : 0)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static string BuildContentEditorUrl(string instanceUrl, string itemId, string? locale)
    {
        var url = $"{instanceUrl}/sitecore/shell/Applications/Content%20Editor.aspx?fo={Uri.EscapeDataString(itemId)}";
        return string.IsNullOrEmpty(locale) ? url : $"{url}&lang={Uri.EscapeDataString(locale)}";
    }

    private sealed record ParsedUpload(
        string? ItemId,
        Dictionary<string, string> Fields,
        string MediaType,
        Func<BlackbirdItemMetadata, byte[]> WriteMetadata);
}
