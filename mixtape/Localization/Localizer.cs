using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace Mixtape.Localization;

public abstract class Localizer : ILocalizer
{
  protected ConcurrentDictionary<string, string> Cache { get; } = new();

  protected ICultureResolver CultureResolver { get; }

  protected string LanguageCode { get; set; }


  protected Localizer(ICultureResolver cultureResolver)
  {
    CultureResolver = cultureResolver;
    CultureResolver.Subscribe(msg => OnCultureChange(msg.Culture));
    OnCultureChange(CultureResolver.Current);
  }


  Task OnCultureChange(CultureInfo culture)
  {
    culture ??= CultureInfo.InvariantCulture;
    LanguageCode = culture.Name.Split(['_', '-'])[0];
    Cache.Clear();
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public string Text(string key) => Text(key, null);


  /// <inheritdoc />
  public string Text(string key, Dictionary<string, string> tokens)
  {
    if (key.IsNullOrEmpty())
    {
      return null;
    }

    if (!Cache.TryGetValue(key, out string value))
    {
      Translation translation = LoadTranslation(key);

      if (translation == null)
      {
        return null;
      }

      value = translation.Value;
      Cache.TryAdd(key, value);
    }

    if (tokens != null)
    {
      value = TokenReplacement.Apply(value, tokens);
    }

    return value;
  }


  /// <inheritdoc />
  public string Text<T>(T enumValue) where T : Enum => Text(enumValue, null);


  /// <inheritdoc />
  public string Text<T>(T enumValue, Dictionary<string, string> tokens) where T : Enum
  {
    Type type = enumValue.GetType();
    MemberInfo memInfo = type.GetMember(enumValue.ToString())[0];
    return Text(memInfo.GetCustomAttribute<LocalizeAttribute>()?.Key, tokens);
  }


  /// <inheritdoc />
  public string Maybe(string key) => Maybe(key, null);


  /// <inheritdoc />
  public string Maybe(string key, Dictionary<string, string> tokens)
  {
    if (key.IsNullOrEmpty() || !key.StartsWith("@"))
    {
      string value = key;
      if (tokens != null)
      {
        value = TokenReplacement.Apply(value, tokens);
      }
      return value;
    }

    return Text(key.Substring(1), tokens);
  }


  /// <summary>
  /// Get translation from database or any other source
  /// </summary>
  protected abstract Translation LoadTranslation(string key);
}

public interface ILocalizer
{
  /// <summary>
  /// Get a text string from a key in the dictionary
  /// </summary>
  string Text(string key);

  /// <summary>
  /// Get a text string from a key in the dictionary (with token replacement)
  /// </summary>
  string Text(string key, Dictionary<string, string> tokens);

  /// <summary>
  /// Get a text string from a [Localize] attribute
  /// </summary>
  string Text<T>(T enumValue) where T : Enum;

  /// <summary>
  /// Get a text string from a [Localize] attribute (with token replacement)
  /// </summary>
  string Text<T>(T enumValue, Dictionary<string, string> tokens) where T : Enum;

  /// <summary>
  /// Only tries to resolve the key when it is prefixed with an @
  /// </summary>
  string Maybe(string key);

  /// <summary>
  /// Only tries to resolve the key when it is prefixed with an @ (with token replacement)
  /// </summary>
  string Maybe(string key, Dictionary<string, string> tokens);
}
