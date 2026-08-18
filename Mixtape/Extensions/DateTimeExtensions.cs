namespace Mixtape.Extensions;

public static class DateTimeExtensions
{
  public static long ToUnixTime(this DateTime dateTime)
  {
    return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
  }
}