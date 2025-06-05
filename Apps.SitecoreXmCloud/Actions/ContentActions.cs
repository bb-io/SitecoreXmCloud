using System.Net.Mime;
using System.Text;
using System.Web;
using Apps.Sitecore.Api;
using Apps.Sitecore.Invocables;
using Apps.Sitecore.Models;
using Apps.Sitecore.Models.Requests.Item;
using Apps.Sitecore.Models.Responses.Item;
using Apps.Sitecore.Utils;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.Sdk.Utils.Extensions.String;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.SitecoreXmCloud.Models;
using Apps.SitecoreXmCloud.Models.Requests.Item;
using Apps.SitecoreXmCloud.Utils;

namespace Apps.Sitecore.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : SitecoreInvocable(invocationContext)
{
    [Action("Download item content", Description = "Get content of the specific item in a file")]
    public async Task<FileModel> GetItemContent([ActionParameter] ItemContentRequest input, [ActionParameter] FileFormatInput format,
        [ActionParameter] FilteringOptions filter)
    {
        var endpoint = "/Content".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Get, Creds);

        List<FieldModel> response;
        try
        {
            response = await Client.ExecuteWithErrorHandling<List<FieldModel>>(request);
        }
        catch
        {
            var oldresponse = await Client.ExecuteWithErrorHandling<Dictionary<string, string>>(request);
            response = oldresponse.Select(x => new FieldModel { ID = x.Key, Value = x.Value }).ToList();
        }

        if (filter.Section != null)
        {
            response = response.Where(x => !filter.Section.Contains(x.Section)).ToList();
        }

        if (filter.Key != null)
        {
            response = response.Where(x => !filter.Key.Contains(x.Key)).ToList();
        }

        if (filter.TypeKey != null)
        {
            response = response.Where(x => !filter.TypeKey.Contains(x.TypeKey)).ToList();
        }

        if (filter.Name != null)
        {
            response = response.Where(x => !filter.Name.Contains(x.Name)).ToList();
        }

        if (filter.Type != null)
        {
            response = response.Where(x => !filter.Type.Contains(x.Type)).ToList();
        }

        if (filter.DisplayName != null)
        {
            response = response.Where(x => !filter.DisplayName.Contains(x.DisplayName)).ToList();
        }

        if (filter.Value != null)
        {
            response = response.Where(x => !filter.Value.Contains(x.Value)).ToList();
        }

        if (filter.Title != null)
        {
            response = response.Where(x => !filter.Title.Contains(x.Title)).ToList();
        }

        if (filter.Description != null)
        {
            response = response.Where(x => !filter.Description.Contains(x.Description)).ToList();
        }

        if (filter.Definition != null)
        {
            response = response.Where(x => !filter.Definition.Contains(x.Definition)).ToList();
        }

        if (filter.SectionDisplayName != null)
        {
            response = response.Where(x => !filter.SectionDisplayName.Contains(x.SectionDisplayName)).ToList();
        }

        if (format.Format == "html")
        {
            var html = SitecoreHtmlConverter.ToHtml(response, input.ItemId);
            var file = await fileManagementClient.UploadAsync(new MemoryStream(html), MediaTypeNames.Text.Html,
                $"{input.ItemId}.html");
            return new()
            {
                File = file
            };
        }
        else if (format.Format == "json")
        {
            var json = SitecoreJsonConverter.GetJsonBytes(response, input.ItemId);

            var file = await fileManagementClient.UploadAsync(new MemoryStream(json), MediaTypeNames.Application.Json,
                $"{input.ItemId}.json");
            return new()
            {
                File = file
            };
        }

        return new FileModel();
    }

    [Action("Upload item content", Description = "Update content of the specific item from a file")]
    public async Task UpdateItemContent(
        [ActionParameter] ItemContentOptionalRequest itemContent,
        [ActionParameter] FileModel file,
        [ActionParameter] UpdateItemContentRequest input)
    {
        var fileStream = await fileManagementClient.DownloadAsync(file.File);
        var bytes = await fileStream.GetByteData();
        string extractedItemId = "";
        var sitecoreFields = new Dictionary<string, string>();

        if (file.File.Name.EndsWith(".html"))
        {
            var html = Encoding.UTF8.GetString(bytes);
            extractedItemId = SitecoreHtmlConverter.ExtractItemIdFromHtml(html);
            sitecoreFields = SitecoreHtmlConverter.ToSitecoreFields(bytes);
        }
        else if (file.File.Name.EndsWith(".json"))
        {
            extractedItemId = await SitecoreJsonConverter.ExtractItemIdFromJson(bytes);
            sitecoreFields = await SitecoreJsonConverter.ExtractFromJsonAsync(bytes);
        }

        var itemId = itemContent.ItemId ?? extractedItemId ?? throw new PluginMisconfigurationException("Didn't find item Item ID in the HTML file. Please provide it in the input.");
        itemContent.ItemId = itemId;

        if (input.AddNewVersion is true)
        {
            itemContent.Version = null;
            await CreateItemContent(new ItemContentRequest { ItemId = itemContent.ItemId, Version = itemContent.Version, Locale = itemContent.Locale });
        }
        var endpoint = "/Content".WithQuery(itemContent);
        var request = new SitecoreRequest(endpoint, Method.Put, Creds);

        sitecoreFields.ToList().ForEach(x =>
            request.AddParameter($"fields[{x.Key}]", HttpUtility.UrlEncode(HttpUtility.UrlEncode(x.Value))));
        await Client.ExecuteWithErrorHandling(request);
    }

    [Action("Get Item ID from file", Description = "Extract Item ID from file")]
    public async Task<GetItemIdFromHtmlResponse> GetItemIdFromHtml([ActionParameter] FileModel file)
    {
        var htmlStream = await fileManagementClient.DownloadAsync(file.File);
        var bytes = await htmlStream.GetByteData();
        string itemId = "";

        if (file.File.Name.EndsWith(".html"))
        {
            var html = Encoding.UTF8.GetString(bytes);
            itemId = SitecoreHtmlConverter.ExtractItemIdFromHtml(html);
        }
        else if (file.File.Name.EndsWith(".json"))
        {
            itemId = await SitecoreJsonConverter.ExtractItemIdFromJson(bytes);
        }

        return new GetItemIdFromHtmlResponse
        {
            ItemId = itemId ?? string.Empty
        };
    }

    [Action("Delete item content", Description = "Delete specific version of item's content")]
    public Task DeleteItemContent([ActionParameter] ItemContentRequest input)
    {
        var endpoint = "/Content".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Delete, Creds);

        return Client.ExecuteWithErrorHandling(request);
    }

    private Task CreateItemContent([ActionParameter] ItemContentRequest input)
    {
        var endpoint = "/Content".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Post, Creds);

        return Client.ExecuteWithErrorHandling(request);
    }
}