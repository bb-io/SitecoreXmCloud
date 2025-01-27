using Apps.Sitecore.Models.Entities;

namespace Apps.Sitecore.Models.Responses.Locale;

public record ListLocalesResponse(IEnumerable<LocaleEntity> Locales);