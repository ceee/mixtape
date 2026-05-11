using System.Reflection;

namespace Mixtape.Utils;

public class ObjectTraverser
{
  private const string SEPARATOR = ".";

  private const string SQBKT_OPEN = "[";

  private const string SQBKT_CLOSE = "]";

  private const string NEWTONSOFT_NS = "Newtonsoft.Json";


  /// <summary>
  /// Find all instances of a given type in an object (features nested + enumerable content)
  /// </summary>
  public static List<Result<T>> Find<T>(object value) where T : class
  {
    HashSet<object> exploredObjects = [];
    List<Result<T>> found = [];

    Find<T>(value, null, string.Empty, null, exploredObjects, found);

    return found;
  }


  /// <summary>
  /// Find all instances of a given type in an object (features nested + enumerable content)
  /// </summary>
  public static List<Result> Find(Type type, object value)
  {
    HashSet<object> exploredObjects = [];
    List<Result> found = [];

    Find(type, value, null, string.Empty, null, exploredObjects, found);

    return found;
  }


  /// <summary>
  /// Find all instances of a given type in an object (features nested + enumerable content)
  /// </summary>
  public static List<Result<T>> FindAttribute<T>(object value) where T : Attribute
  {
    HashSet<string> exploredObjects = [];
    List<Result<T>> found = [];

    FindAttribute<T>(value, null, string.Empty, null, exploredObjects, found);

    return found;
  }


  /// <summary>
  /// Recursively ind all instances of a given type in an object
  /// </summary>
  private static void Find<T>(object value, object parent, string key, PropertyInfo property, HashSet<object> exploredObjects, List<Result<T>> found) where T : class
  {
    if (value == null || value is string || exploredObjects.Contains(value) || value.GetType().FullName.StartsWith(NEWTONSOFT_NS))
    {
      return;
    }

    exploredObjects.Add(value);

    System.Collections.ICollection enumerable = value as System.Collections.ICollection;

    // this property is a list
    if (enumerable != null)
    {
      int idx = 0;
      foreach (object item in enumerable)
      {
        Find<T>(item, value, CombineKey(string.Empty, key, SQBKT_OPEN + idx + SQBKT_CLOSE), property, exploredObjects, found);
        idx += 1;
      }
      return;
    }

    // get type of value
    Type type = typeof(T);
    Type valueType = value.GetType();
    Type genericValueType = valueType.IsGenericType ? valueType.GetGenericTypeDefinition() : null;

    // check if the current property is our searched type
    if (valueType == type || genericValueType == type || type.IsAssignableFrom(valueType) || (genericValueType != null && type.IsAssignableFrom(genericValueType)))
    {
      found.Add(new Result<T>()
      {
        Path = key,
        Item = value as T,
        Parent = parent,
        Property = property
      });
      return;
    }

    // check if our property is a primitive type
    if (value == null || valueType.IsPrimitive || value is string || value is DateTime || value is DateTimeOffset || value is Uri)
    {
      return;
    }

    // iterate over nested properties
    PropertyInfo[] properties = valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
    foreach (PropertyInfo nestedProperty in properties)
    {
      object propertyValue = nestedProperty.GetValue(value, null);
      Find<T>(propertyValue, value, CombineKey(SEPARATOR, key, nestedProperty.Name), nestedProperty, exploredObjects, found);
    }
  }


  /// <summary>
  /// Recursively ind all instances of a given type in an object
  /// </summary>
  private static void Find(Type type, object value, object parent, string key, PropertyInfo property, HashSet<object> exploredObjects, List<Result> found)
  {
    if (value == null || value is string || exploredObjects.Contains(value) || value.GetType().FullName.StartsWith(NEWTONSOFT_NS))
    {
      return;
    }

    exploredObjects.Add(value);

    System.Collections.ICollection enumerable = value as System.Collections.ICollection;

    // this property is a list
    if (enumerable != null)
    {
      int idx = 0;
      foreach (object item in enumerable)
      {
        Find(type, item, value, CombineKey(string.Empty, key, SQBKT_OPEN + idx + SQBKT_CLOSE), property, exploredObjects, found);
        idx += 1;
      }
      return;
    }

    // get type of value
    Type valueType = value.GetType();
    Type genericValueType = valueType.IsGenericType ? valueType.GetGenericTypeDefinition() : null;

    // check if the current property is our searched type
    if (valueType == type || genericValueType == type || type.IsAssignableFrom(valueType) || (genericValueType != null && type.IsAssignableFrom(genericValueType)))
    {
      found.Add(new Result()
      {
        Path = key,
        Item = value,
        Type = valueType,
        Parent = parent,
        Property = property
      });
      return;
    }

    // check if our property is a primitive type    
    if (value == null || valueType.IsPrimitive || value is string || value is DateTime || value is DateTimeOffset || value is Uri)
    {
      return;
    }

    // iterate over nested properties
    PropertyInfo[] properties = valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
    foreach (PropertyInfo nestedProperty in properties)
    {
      object propertyValue = nestedProperty.GetValue(value, null);
      Find(type, propertyValue, value, CombineKey(SEPARATOR, key, nestedProperty.Name), nestedProperty, exploredObjects, found);
    }
  }


  /// <summary>
  /// Recursively find all instances with the attribute of a given type in an object
  /// </summary>
  private static void FindAttribute<T>(object value, object parent, string key, PropertyInfo property, HashSet<string> exploredObjects, List<Result<T>> found) where T : Attribute
  {
    if (exploredObjects.Contains(key) || (value != null && value.GetType().FullName.StartsWith(NEWTONSOFT_NS)))
    {
      return;
    }

    exploredObjects.Add(key);

    // check if the current property contains the attribute
    if (property != null)
    {
      T attribute = property.GetCustomAttribute<T>(true);

      if (attribute != null)
      {
        found.Add(new Result<T>()
        {
          Path = key,
          Item = attribute,
          Parent = parent,
          Property = property //parent.GetType().GetProperty(key)
        });
        return;
      }
    }

    System.Collections.ICollection enumerable = value as System.Collections.ICollection;

    // this property is a list
    if (enumerable != null && !(value is string))
    {
      int idx = 0;
      foreach (object item in enumerable)
      {
        FindAttribute<T>(item, value, CombineKey(string.Empty, key, SQBKT_OPEN + idx + SQBKT_CLOSE), null, exploredObjects, found);
        idx += 1;
      }
      return;
    }

    // check if our property is a primitive type
    if (value == null || value.GetType().IsPrimitive || value is string || value is DateTime || value is DateTimeOffset || value is Uri || value is System.Text.Json.JsonElement)
    {
      return;
    }

    // iterate over nested properties
    PropertyInfo[] properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
    foreach (PropertyInfo nestedProperty in properties)
    {
      object propertyValue = nestedProperty.GetValue(value, null);
      FindAttribute<T>(propertyValue, value, CombineKey(SEPARATOR, key, nestedProperty.Name), nestedProperty, exploredObjects, found);
    }
  }


  private static string CombineKey(string sep, string key1, string key2)
  {
    if (string.IsNullOrEmpty(key1))
    {
      return key2;
    }
    return string.Join(sep, key1, key2);
  }


  public class Result<T>
  {
    public string Path { get; set; }

    public PropertyInfo Property { get; set; }

    public object Parent { get; set; }

    public T Item { get; set; }
  }

  public class Result
  {
    public string Path { get; set; }

    public PropertyInfo Property { get; set; }

    public object Parent { get; set; }

    public object Item { get; set; }

    public Type Type { get; set; }

    public int Index { get; set; }
  }
}
