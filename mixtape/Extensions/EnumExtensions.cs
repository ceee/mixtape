namespace Mixtape.Extensions;

public static class EnumExtensions
{
  /// <summary>
  /// Get unique flags from an enum
  /// </summary>
  public static IEnumerable<T> GetFlags<T>(this T flags) where T : struct, Enum
  {
    return Enum.GetValues<T>().Where(member => flags.HasFlag(member)).ToArray();
  }
}