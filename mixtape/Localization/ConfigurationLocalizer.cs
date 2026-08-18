using Microsoft.Extensions.Options;

namespace Mixtape.Localization;

public class ConfigurationLocalizer(ConfigurationLocalizationCache cache, ICultureResolver cultureResolver, IOptions<LocalizationOptions> options) : Localizer(cultureResolver, options)
{
  protected override Translation LoadTranslation(string key)
  {
    return cache.Get(LanguageCode).GetValueOrDefault(key);
  }
}