namespace Mixtape.Extensions;

public static class NumberExtensions
{
  static Dictionary<FileSizeNotation, string[]> FileSizeUnits = new()
  {
    { FileSizeNotation.SI, ["B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"] },
    { FileSizeNotation.IEC, ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"] },
    { FileSizeNotation.JEDEC, ["B", "KB", "MB", "GB"] }
  };

  public static int Limit(this int input, int min, int max)
  {
    if (input < min)
    {
      return min;
    }
    if (input > max)
    {
      return max;
    }
    return input;
  }


  public static string GetFileSize(this long sizeInBytes, FileSizeNotation notation = FileSizeNotation.JEDEC)
  {
    if (!FileSizeUnits.ContainsKey(notation))
    {
      throw new NotImplementedException($"The notation {notation} has no implementation for generating human-readable file sizes");
    }

    int power = notation == FileSizeNotation.SI ? 1000 : 1024;

    string[] units = FileSizeUnits[notation];

    int order = 0;

    while (sizeInBytes >= power && order + 1 < units.Length)
    {
      order++;
      sizeInBytes = sizeInBytes / power;
    }

    return string.Format("{0:0.##} {1}", sizeInBytes, units[order]);
  }
}
