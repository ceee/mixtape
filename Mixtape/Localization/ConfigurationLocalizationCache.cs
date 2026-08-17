using Microsoft.Extensions.Options;

namespace Mixtape.Localization;

public class ConfigurationLocalizationCache : IDisposable
{
  private readonly IDisposable _subscription;

  private volatile Dictionary<string, Dictionary<string, Translation>> _languages = [];

  
  public ConfigurationLocalizationCache(IOptionsMonitor<LocalizationOptions> options)
  {
    Rebuild(options.CurrentValue);
    _subscription = options.OnChange((opts, _) => Rebuild(opts));
  }

  
  public IReadOnlyDictionary<string, Translation> Get(string languageCode)
  {
    return _languages.GetValueOrDefault(languageCode) ?? [];
  }

  
  private void Rebuild(LocalizationOptions options)
  {
    Dictionary<string, Dictionary<string, Translation>> languages = new(options.Count);

    foreach ((string language, Dictionary<string, string> values) in options)
    {
      Dictionary<string, Translation> cache = new(values.Count);
      foreach ((string key, string value) in values)
      {
        cache.Add(key, new() { Value = value });
      }
      languages.Add(language, cache);
    }

    _languages = languages;
  }

  
  public void Dispose()
  {
    _subscription.Dispose();
  }
}