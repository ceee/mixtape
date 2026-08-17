namespace Mixtape.Localization;

public class ConfigurationLocalizer(ConfigurationLocalizationCache cache, ICultureResolver cultureResolver) : Localizer(cultureResolver)
{
  protected override Translation LoadTranslation(string key)
  {
    return cache.Get(LanguageCode).GetValueOrDefault(key);
  }
}