using Microsoft.AspNetCore.Html;

namespace Mixtape.Localization;

public static class LocalizerExtensions
{
  extension(ILocalizer localizer)
  {
    /// <summary>
    /// Get HTML from a key in the dictionary
    /// </summary>
    public IHtmlContent Html(string key)
    {
      string value = localizer.Text(key);

      HtmlContentBuilder builder = new();
      builder.SetHtmlContent(value);
      return builder;
    }

    /// <summary>
    /// Get HTML from a key in the dictionary (with token replacement)
    /// </summary>
    public IHtmlContent Html(string key, Dictionary<string, string> tokens)
    {
      string value = localizer.Text(key, tokens);

      HtmlContentBuilder builder = new();
      builder.SetHtmlContent(value);
      return builder;
    }

    /// <summary>
    /// Get HTML (escaped entities) from a key in the dictionary (with token replacement)
    /// </summary>
    public IHtmlContent HtmlEntities(string key, Dictionary<string, string> tokens = null)
    {
      string value = localizer.Text(key, tokens);

      HtmlContentBuilder builder = new();
      builder.SetHtmlContent(value.ToHtmlEntities());
      return builder;
    }
    
    /// <summary>
    /// Get a text string from a key in the dictionary (with token replacement)
    /// </summary>
    public string Text(string key, params (string key, string value)[] tokens)
    {
      return localizer.Text(key, tokens.ToDictionary(x => x.key, x => x.value.ToString()));
    }

    /// <summary>
    /// Get a text string from a [Localize] attribute (with token replacement)
    /// </summary>
    public string Text<T>(T enumValue, params (string key, string value)[] tokens) where T : Enum
    {
      return localizer.Text(enumValue, tokens.ToDictionary(x => x.key, x => x.value.ToString()));
    }
    
    /// <summary>
    /// Only tries to resolve the key when it is prefixed with an @ (with token replacement)
    /// </summary>
    public string Maybe(string key, params (string key, string value)[] tokens)
    {
      return localizer.Maybe(key, tokens.ToDictionary(x => x.key, x => x.value.ToString()));
    }
    
    /// <summary>
    /// Get HTML from a key in the dictionary (with token replacement)
    /// </summary>
    public IHtmlContent Html(string key, params (string key, string value)[] tokens)
    {
      return localizer.Html(key, tokens.ToDictionary(x => x.key, x => x.value.ToString()));
    }

    /// <summary>
    /// Get HTML (escaped entities) from a key in the dictionary (with token replacement)
    /// </summary>
    public IHtmlContent HtmlEntities(string key, params (string key, string value)[] tokens)
    {
      return localizer.HtmlEntities(key, tokens.ToDictionary(x => x.key, x => x.value.ToString()));
    }
  }
}