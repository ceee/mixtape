using System.IO;
using System.Text;

namespace Mixtape.Utils;

public static class Safenames
{
  public enum Scope
  {
    Url,
    File,
    Tag
  }
  
  const char Underscore = '_';
  const char Hyphen = '-';
  const char Dot = '.';
  const char Plus = '+';
  const char Ampersand = '&';
  static readonly char[] Ticks = ['`', '\'', '´'];


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
  /// Converts an untrusted to a safe tag ([a-z][0-9][-][_])
  /// </summary>
  public static string Tag(string value)
  {
    return Generate(value, Scope.Tag);
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

    char previous = '\0';
    StringBuilder output = new();

    foreach (char t in value)
    {
      // get character in lower case
      char character = char.ToLower(t);
      char target;

      // do not handle surrogates
      if (char.IsSurrogate(character))
      {
        continue;
      }
      
      // do not handle ticks
      if (Ticks.Contains(character))
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
      // + sign for + and &
      else if (scope is not Scope.Tag && character is Plus or Ampersand)
      {
        target = Plus;
      }
      else if (scope == Scope.File && character == Dot)
      {
        target = Dot;
      }
      else if (scope is Scope.Tag && character == Underscore)
      {
        target = Underscore;
      }
      // add hyphen for all other characters
      else
      {
        target = Hyphen;
      }

      // add default characters
      if (target != Hyphen && target != Plus)
      {
        output.Append(target);
      }
      // add hyphen if it isn't first and previous char is not + or -
      else if (target == Hyphen && previous != 0 && previous != Plus && previous != Hyphen)
      {
        output.Append(target);
      }
      // add plus. do remove hyphen it is the previous character
      else if (target == Plus)
      {
        if (previous == Hyphen)
        {
          output.Remove(output.Length - 1, 1);
        }
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
      output.Append(Hyphen);
    }

    return output.ToString();
  }
}