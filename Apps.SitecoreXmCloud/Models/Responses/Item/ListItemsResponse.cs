using Apps.Sitecore.Models.Entities;
using Blackbird.Applications.SDK.Blueprints.Interfaces.CMS;

namespace Apps.Sitecore.Models.Responses.Item;

public record ListItemsResponse(IEnumerable<ItemEntity> Items) : ISearchContentOutput<ItemEntity>;