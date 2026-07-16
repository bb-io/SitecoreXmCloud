using System.Net.Mime;
using System.Text;
using System.Web;
using Apps.Sitecore.Api;
using Apps.Sitecore.Constants;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models;
using Apps.Sitecore.Models.Entities;
using Apps.Sitecore.Models.Requests.Item;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Utils;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Entities;
using Apps.SitecoreXmCloud.Models.Requests.Item;
using Apps.SitecoreXmCloud.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.Sdk;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using Blackbird.Filters.Transformations;
using Blackbird.Filters.Xliff.Xliff2;
using RestSharp;

namespace Apps.Sitecore.Actions;

[ActionList("Content")]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : SitecoreInvocable(invocationContext)
{
    private const string HtmlExtension = ".html";
    private const string JsonExtension = ".json";

    [Action("Search content", Description = "Search content based on provided criteria")]
    [BlueprintActionDefinition(BlueprintAction.SearchContent)]
    public async Task<ListItemsResponse> SearchItems([ActionParameter] SearchItemsRequest input)
    {
        var endpoint = "/Search".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Get, Creds);

        var items = await Client.Paginate<ItemEntity>(request);

        var latestItems = items
            .GroupBy(item => item.Id)
            .Select(g => g.OrderByDescending(item => int.TryParse(item.Version, out var v) ? v : 0)
                .FirstOrDefault())
            .ToList();

        return new ListItemsResponse(latestItems);
    }

    [Action("Download content", Description = "Get localizable fields of the specific item")]
    [BlueprintActionDefinition(BlueprintAction.DownloadContent)]
    public async Task<FileModel> GetItemContent([ActionParameter] ItemContentRequest input,
        [ActionParameter] FileFormatInput format,
        [ActionParameter] FilteringOptions filter)
    {
        var fields = ApplyFilters(await FetchItemFields(input), filter);
        var metadata = await BuildItemMetadataAsync(input.ContentId, input.Locale);

        return format.Format switch
        {
            "html" => await UploadFile(SitecoreHtmlConverter.ToHtml(fields, metadata),
                MediaTypeNames.Text.Html, $"{input.ContentId}.html"),
            "json" => await UploadFile(SitecoreJsonConverter.ToJson(fields, metadata),
                MediaTypeNames.Application.Json, $"{input.ContentId}.json"),
            _ => new FileModel()
        };
    }

    [Action("Upload content", Description = "Upload localizable fields to the specific item from a file")]
    [BlueprintActionDefinition(BlueprintAction.UploadContent)]
    public async Task<FileModel> UpdateItemContent(
        [ActionParameter] UploadContentRequest uploadContentRequest,
        [ActionParameter] UpdateItemContentRequest input)
    {
        var fileStream = await fileManagementClient.DownloadAsync(uploadContentRequest.Content);
        var bytes = await fileStream.GetByteData();
        var payload = LoadUploadPayload(uploadContentRequest.Content.Name, bytes);

        uploadContentRequest.ContentId = uploadContentRequest.ContentId ?? payload.ExtractedItemId
            ?? throw new PluginMisconfigurationException(
                "Didn't find item Item ID in the HTML file. Please provide it in the input.");

        if (input.AddNewVersion is true)
        {
            uploadContentRequest.Version = null;
            await CreateItemContent(new ItemContentRequest
            {
                ContentId = uploadContentRequest.ContentId,
                Version = uploadContentRequest.Version,
                Locale = uploadContentRequest.Locale
            });
        }

        var request = new SitecoreRequest("/Content", Method.Put, Creds);
        if (!string.IsNullOrEmpty(uploadContentRequest.Locale))
        {
            request.AddParameter("locale", uploadContentRequest.Locale);
        }
        if (!string.IsNullOrEmpty(uploadContentRequest.ContentId))
        {
            request.AddParameter("itemId", uploadContentRequest.ContentId);
        }
        if (!string.IsNullOrEmpty(uploadContentRequest.Version))
        {
            request.AddParameter("version", uploadContentRequest.Version);
        }
        foreach (var pair in payload.Fields)
        {
            request.AddParameter($"fields[{pair.Key}]",
                HttpUtility.UrlEncode(HttpUtility.UrlEncode(pair.Value)));
        }
        await Client.ExecuteWithErrorHandling(request);

        var metadata = await BuildItemMetadataAsync(uploadContentRequest.ContentId, uploadContentRequest.Locale);
        return await BuildTargetFileAsync(uploadContentRequest.Content, payload, metadata);
    }

    [Action("Get IDs from item content", Description = "Get Item ID from the HTML or JSON content file")]
    public async Task<GetItemIdFromHtmlResponse> GetItemIdFromHtml([ActionParameter] FileModel file)
    {
        var fileStream = await fileManagementClient.DownloadAsync(file.Content);
        var bytes = await fileStream.GetByteData();

        var fileContent = Encoding.UTF8.GetString(bytes);
        var itemId = file.Content.Name switch
        {
            var n when n.EndsWith(HtmlExtension, StringComparison.OrdinalIgnoreCase) =>
                SitecoreHtmlConverter.ExtractItemIdFromHtml(fileContent),
            var n when n.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase) =>
                await SitecoreJsonConverter.ExtractItemIdFromJson(fileContent),
            _ => null
        };

        return new GetItemIdFromHtmlResponse { ItemId = itemId ?? string.Empty };
    }

    [Action("Delete content", Description = "Delete specific version of item's content")]
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
        return new FileModel { Content = uploaded };
    }

    private static UploadPayload LoadUploadPayload(string fileName, byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        Transformation? transformation = null;

        if (Xliff2Serializer.IsXliff2(text))
        {
            transformation = Transformation.Parse(text, fileName);
            text = transformation.Target().Serialize()
                ?? throw new PluginMisconfigurationException("XLIFF did not contain any files");
        }

        if (fileName.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase))
        {
            var root = SitecoreJsonConverter.Parse(text);
            return new UploadPayload(
                transformation,
                SitecoreJsonConverter.ExtractItemId(root),
                SitecoreJsonConverter.ExtractFields(root),
                MediaTypeNames.Application.Json,
                metadata => SitecoreJsonConverter.WriteMetadata(root, metadata));
        }

        // Default to HTML — covers .html files and XLIFF-serialised-to-HTML content.
        var doc = SitecoreHtmlConverter.Parse(text);
        return new UploadPayload(
            transformation,
            SitecoreHtmlConverter.ExtractItemId(doc),
            SitecoreHtmlConverter.ExtractFields(doc),
            MediaTypeNames.Text.Html,
            metadata => SitecoreHtmlConverter.WriteMetadata(doc, metadata));
    }

    private async Task<FileModel> BuildTargetFileAsync(
        Blackbird.Applications.Sdk.Common.Files.FileReference sourceFile,
        UploadPayload payload,
        BlackbirdItemMetadata metadata)
    {
        if (payload.Transformation is not null)
        {
            TransformationTargetMetadata.ApplySitecoreTarget(payload.Transformation, metadata);
            return await UploadFile(
                Encoding.UTF8.GetBytes(payload.Transformation.Serialize()),
                MediaTypeNames.Text.Html,
                payload.Transformation.XliffFileName);
        }

        var updatedBytes = payload.WriteMetadata(metadata);
        return await UploadFile(updatedBytes, payload.MediaType, sourceFile.Name);
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

    private sealed record UploadPayload(
        Transformation? Transformation,
        string? ExtractedItemId,
        Dictionary<string, string> Fields,
        string MediaType,
        Func<BlackbirdItemMetadata, byte[]> WriteMetadata);
}
