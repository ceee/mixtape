namespace Mixtape.Extensions;

public static class CharExtensions
{
  private static Dictionary<char, char[]> accents { get; } = new Dictionary<char, char[]>()
  {
    { 'ä', ['a', 'e'] },
    { 'á', ['a'] },
    { 'à', ['a'] },
    { 'ó', ['o'] },
    { 'ò', ['o'] },
    { 'é', ['e'] },
    { 'è', ['e'] },
    { 'ú', ['u'] },
    { 'ù', ['u'] },
    { 'í', ['i'] },
    { 'ì', ['i'] },
    { 'ö', ['o', 'e'] },
    { 'ü', ['u', 'e'] },
    { 'ß', ['s', 's'] },
    //{ '&', new char[1] { '+' } }
  };


  /// <summary>
  /// Check if a character is from a-z, A-Z or 0-9
  /// </summary>
  public static bool IsAZor09(this char value)
  {
    return (value >= 0x41 && value <= 0x5A) || (value >= 0x61 && value <= 0x7a) || (value >= 0x30 && value <= 0x39);
  }


  /// <summary>
  /// Check if a character is in ASCII range
  /// </summary>
  public static bool IsASCII(this char value)
  {
    return value < 128;
  }


  /// <summary>
  /// Replaces an accent or umlaut with the appropriate URL + file ready variant
  /// </summary>
  public static bool TryReplaceAccent(this char value, out char[] result)
  {
    if (!accents.ContainsKey(value))
    {
      result = null;
      return false;
    }

    result = accents[value];
    return true;
  }
}
