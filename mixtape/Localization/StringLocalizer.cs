using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Mixtape.Localization;

public class StringLocalizer<T>(ILocalizer localizer) : StringLocalizer(localizer), IStringLocalizer<T>;


public class StringLocalizer(ILocalizer localizer) : IStringLocalizer
{
  public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
  {
    return new List<LocalizedString>();
  }

  public LocalizedString this[string name] => new(name, GetString(name), false);

  
  public LocalizedString this[string name, params object[] arguments] =>
    new(name, string.Format(GetString(name), arguments), false);

  
  // public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
  // {
  //   return _stringProvider.GetAllStrings(CultureInfo.CurrentCulture.Name);
  // }

  
  public IStringLocalizer WithCulture(CultureInfo culture)
  {
    CultureInfo.DefaultThreadCurrentCulture = culture;
    return new StringLocalizer(localizer);
  }

  private string GetString(string name)
  {
    string cultureName = CultureInfo.CurrentCulture.Name;
    string value = localizer.Text(name);

    if (string.IsNullOrEmpty(value))
    {
      // string[] parts = cultureName.Split('-');
      // if (parts[0] != cultureName)
      // {
      //   value = _localizer.Text(name, parts[0]);
      // }
    }

    return value;
  }
}