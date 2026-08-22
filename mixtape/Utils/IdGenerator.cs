using System.Text.Json;

namespace Mixtape.Utils;

public class IdGenerator
{
  const string CharsAz09 = "abcdefghijklmnopqrstuvwxyz0123456789";
  const string CharsAzAz09X = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-@#.:!?*";
  const string CharsX09 = "0123456789";

  private static readonly Random Random = new();

  /// <summary>
  /// Create a new unique Id
  /// </summary>
  public static string Create(int length = -1, Charset charset = Charset.az09)
  {
    if (length < 1)
    {
      length = 12;
    }

    string chars = charset == Charset.az09 ? CharsAz09 : charset == Charset.x09 ? CharsX09 : CharsAzAz09X;

    return new(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());

    //if (length > 0)
    //{
    //return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
    //  .Replace("/", String.Empty)
    //  .Replace("+", String.Empty)
    //  .Replace("-", String.Empty)
    //  .ToLowerInvariant()
    //  .Substring(0, length);
    //}

    //return Guid.NewGuid().ToString();
  }


  /// <summary>
  /// Creates a simple hash from a string
  /// </summary>
  [Obsolete("Use Hashimoto instead")]
  public static string HashString(string value)
  {
    return GetStableHashCode(value).ToString().Replace("-", string.Empty);
  }


  /// <summary>
  /// Creates a simple hash from a string
  /// </summary>
  [Obsolete("Use Hashimoto instead")]
  public static string HashObject(params object[] values)
  {
    return GetStableHashCode(JsonSerializer.Serialize(values)).ToString().Replace("-", string.Empty);
  }


  /// <summary>
  /// Autofill IDs on an object with [GenerateId] attributes
  /// </summary>
  public static T Autofill<T>(T model)
  {
    // find all Raven Ids
    List<ObjectTraverser.Result<GenerateIdAttribute>> ravenIds = ObjectTraverser.FindAttribute<GenerateIdAttribute>(model);

    // set unset Raven Ids
    foreach (ObjectTraverser.Result<GenerateIdAttribute> item in ravenIds)
    {
      string id = item.Property.GetValue(item.Parent, null) as string;
      if (id.IsNullOrWhiteSpace())
      {
        id = item.Item.Length.HasValue ? Create(item.Item.Length.Value) : Create();
        item.Property.SetValue(item.Parent, id);
      }
    }

    return model;
  }


  static int GetStableHashCode(string str)
  {
    unchecked
    {
      int hash1 = 5381;
      int hash2 = hash1;

      for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
      {
        hash1 = ((hash1 << 5) + hash1) ^ str[i];
        if (i == str.Length - 1 || str[i + 1] == '\0')
          break;
        hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
      }

      return hash1 + (hash2 * 1566083941);
    }
  }


  public enum Charset
  {
    /// <summary>
    /// a-z, 0-9
    /// </summary>
    az09 = 0,
    /// <summary>
    /// a-z, A-Z, 0-9, _-@#.:!?*
    /// </summary>
    azAZ09x = 1,
    /// <summary>
    /// 0-9
    /// </summary>
    x09 = 2
  }
}