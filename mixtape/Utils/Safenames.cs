using System.IO;
using System.Text;

namespace Mixtape.Utils;

public class Safenames
{
  public enum Scope
  {
    Url,
    File
  }

  const char HYPHEN = '-';

  const char DOT = '.';

  static char[] TICKS = ['`', '\'', '´'];


  /// <summary>
  /// Converts an untrusted to a safe filename
  /// </summary>
  public static string File(string value)
  {
    return Generate(Path.GetFileName(value), Scope.File);
  }


  /// <summary>
  /// Converts a term to a safe alias (suitable for URLs)
  /// </summary>
  public static string Alias(string value)
  {
    return Generate(value, Scope.Url);
  }


  /// <summary>
  /// Converts a term to a safe alias (suitable for URLs)
  /// </summary>
  public static string Alias(object value)
  {
    return Generate(value?.ToString(), Scope.Url);
  }


  /// <summary>
  /// 
  /// </summary>
  static string Generate(string value, Scope scope)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return string.Empty;
    }

    char previous = default;
    StringBuilder output = new();

    for (int i = 0; i < value.Length; i++)
    {
      // get character in lower case
      char character = char.ToLower(value[i]);
      char target;

      // do not handle surrogates
      if (char.IsSurrogate(character))
      {
        continue;
      }
      // do not handle ticks
      else if (TICKS.Contains(character))
      {
        continue;
      }

      // special replacements accents + umlauts
      if (character.TryReplaceAccent(out char[] replacement))
      {
        if (replacement.Length > 1)
        {
          output.Append(replacement);
          output.Remove(output.Length - 1, 1);
        }
        target = replacement[replacement.Length - 1];
      }
      // append character a-z, 0-9
      else if (character.IsAZor09())
      {
        target = character;
      }
      // - sign for + and &
      else if (scope == Scope.File && character == DOT)
      {
        target = DOT;
      }
      // add hyphen for all other characters
      else
      {
        target = HYPHEN;
      }

      // add default characters
      if (target != HYPHEN)
      {
        output.Append(target);
      }
      // add hyphen if it isn't first and previous char is not + or -
      else if (target == HYPHEN && previous != default(char) && previous != HYPHEN)
      {
        output.Append(target);
      }

      if (output.Length > 0)
      {
        previous = output[output.Length - 1];
      }
    }

    if (output.Length > 0 && !output[output.Length - 1].IsAZor09())
    {
      output.Remove(output.Length - 1, 1);
    }

    if (output.Length == 0)
    {
      output.Append(HYPHEN);
    }

    return output.ToString();
  }
}