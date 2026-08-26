using System.Drawing;

namespace Mixtape.Extensions;

public static class DateExtensions
{
  public const long UnixEpoch = 621355968000000000L;
  private static readonly DateTime UnixEpochDateTimeUtc = new(UnixEpoch, DateTimeKind.Utc);
  private static readonly DateTime MinDateTimeUtc = new(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
  
  
  public static long ToUnixTime(this DateTime date)
  {
    DateTime utcDate = date;
    
    if (date.Kind != DateTimeKind.Utc)
    {
      utcDate = date.Kind == DateTimeKind.Unspecified && date > DateTime.MinValue && date < DateTime.MaxValue
        ? DateTime.SpecifyKind(date.Subtract(TimeZoneInfo.Local.GetUtcOffset(date)), DateTimeKind.Utc)
        : date.ToStableUniversalTime();
    }

    TimeSpan universal = utcDate.Subtract(UnixEpochDateTimeUtc);
    return (long)universal.TotalMilliseconds;
  }
  
  
  public static long ToUnixTime(this DateTimeOffset date)
  {
    TimeSpan universal = date.UtcDateTime.Subtract(UnixEpochDateTimeUtc);
    return (long)universal.TotalMilliseconds;
  }
  
  
  public static DateTime ToStableUniversalTime(this DateTime dateTime)
  {
    if (dateTime.Kind == DateTimeKind.Utc)
    {
      return dateTime;
    }

    return dateTime == DateTime.MinValue ? MinDateTimeUtc : dateTime.ToUniversalTime();
  }
  
  
  extension (DateTimeOffset dateTimeOffset)
  {
    public DateOnly DateOnly => DateOnly.FromDateTime(dateTimeOffset.DateTime);
  }
  
  extension (DateTime dateTime)
  {
    public DateOnly DateOnly => DateOnly.FromDateTime(dateTime);
  }
}
