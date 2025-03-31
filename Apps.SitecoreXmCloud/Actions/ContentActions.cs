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
using HtmlAgilityPack;
using RestSharp;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Apps.SitecoreXmCloud.Models;

namespace Apps.Sitecore.Actions;

[ActionList]
public class ContentActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient)
    : SitecoreInvocable(invocationContext)
{
    [Action("Get item content as HTML", Description = "Get content of the specific item in HTML format")]
    public async Task<FileModel> GetItemContent([ActionParameter] ItemContentRequest input)
    {
        var endpoint = "/Content".WithQuery(input);
        var request = new SitecoreRequest(endpoint, Method.Get, Creds);

        IEnumerable<FieldModel> response;
        try
        {
            response = await Client.ExecuteWithErrorHandling<IEnumerable<FieldModel>>(request);
        }
        catch
        {
            var oldresponse = await Client.ExecuteWithErrorHandling<Dictionary<string, string>>(request);
            response = oldresponse.Select(x => new FieldModel { ID = x.Key, Value = x.Value }).ToArray();
        }

        var html = SitecoreHtmlConverter.ToHtml(response, input.ItemId);

        var file = await fileManagementClient.UploadAsync(new MemoryStream(html), MediaTypeNames.Text.Html,
            $"{input.ItemId}.html");
        return new()
        {
            File = file
        };
    }

    [Action("Update item content from HTML", Description = "Update content of the specific item from HTML file")]
    public async Task UpdateItemContent(
        [ActionParameter] ItemContentOptionalRequest itemContent,
        [ActionParameter] FileModel file,
        [ActionParameter] UpdateItemContentRequest input)
    {
        var htmlStream = await fileManagementClient.DownloadAsync(file.File);
        var bytes = await htmlStream.GetByteData();
        var html = Encoding.UTF8.GetString(bytes);
        var extractedItemId = SitecoreHtmlConverter.ExtractItemIdFromHtml(html);
        
        var itemId = itemContent.ItemId ?? extractedItemId ?? throw new PluginMisconfigurationException("Didn't find item Item ID in the HTML file. Please provide it in the input.");
        itemContent.ItemId = itemId;

        if (input.AddNewVersion is true)
        {
            itemContent.Version = null;
            await CreateItemContent(new ItemContentRequest { ItemId = itemContent.ItemId, Version = itemContent.Version, Locale = itemContent.Locale});
        }

        var sitecoreFields = SitecoreHtmlConverter.ToSitecoreFields(bytes);

        var endpoint = "/Content".WithQuery(itemContent);
        var request = new SitecoreRequest(endpoint, Method.Put, Creds);

        sitecoreFields.ToList().ForEach(x =>
            request.AddParameter($"fields[{x.Key}]", HttpUtility.UrlEncode(HttpUtility.UrlEncode(x.Value))));
        await Client.ExecuteWithErrorHandling(request);
    }
    
    [Action("Get Item ID from HTML", Description = "Extract Item ID from HTML file")]
    public async Task<GetItemIdFromHtmlResponse> GetItemIdFromHtml([ActionParameter] FileModel file)
    {
        var htmlStream = await fileManagementClient.DownloadAsync(file.File);
        var bytes = await htmlStream.GetByteData();
        var html = Encoding.UTF8.GetString(bytes);
        var itemId = SitecoreHtmlConverter.ExtractItemIdFromHtml(html);
        
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