namespace Mixtape.Localization;

public class LocalizationOptions
{
  public string FilePath { get; set; }
  
  public bool CaseInsensitiveKeys { get; set; } = false;
  
  public LocalizationLanguageKeys Keys { get; set; }
}

public class LocalizationLanguageKeys : Dictionary<string, Dictionary<string, string>>
{
  
}