using System.Text.RegularExpressions;

namespace Mixtape.Utils;

public class TokenReplacement
{
  static readonly Regex TokenRegex = new(@"{([\w.-]+)}", RegexOptions.Compiled | RegexOptions.IgnoreCase); // "{([\\w-_.]+)}"


  public static string Apply(string text, IReadOnlyDictionary<string, string> tokens)
  {
    if (text.IsNullOrWhiteSpace() || tokens.Count == 0)
    {
      return text;
    }
    
    return TokenRegex.Replace(text, match =>
    {
      string token = match.Groups[1].Value;
      return tokens.TryGetValue(token, out string value) ? value : string.Empty;
    });
  }
}
