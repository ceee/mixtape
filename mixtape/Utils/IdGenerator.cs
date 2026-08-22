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

    string chars = charset switch
    {
      Charset.az09 => CharsAz09,
      Charset.x09 => CharsX09,
      _ => CharsAzAz09X
    };
    
    return new(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
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