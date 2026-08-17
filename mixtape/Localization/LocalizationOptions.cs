namespace Mixtape.Localization;

public class LocalizationOptions : LocalizationLanguageKeys
{
  public string FilePath { get; set; }
}

public class LocalizationLanguageKeys : Dictionary<string, Dictionary<string, string>>
{
  
}