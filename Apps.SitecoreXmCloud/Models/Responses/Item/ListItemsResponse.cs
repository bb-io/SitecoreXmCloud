using Apps.Sitecore.Models.Entities;

namespace Apps.Sitecore.Models.Responses.Item;

public record ListItemsResponse(IEnumerable<ItemEntity> Items);