namespace Mixtape.Sqlite;

public class InterceptorParameters
{
  /// <summary>
  /// The current mixtape context
  /// </summary>
  public IMixtapeContext Context { get; set; }

  /// <summary>
  /// Mixtape.Sqlite document store
  /// </summary>
  public IMixtapeStore Store { get; set; }

  /// <summary>
  /// Access to other interceptor methods
  /// </summary>
  public IInterceptors Interceptors { get; internal set; }

  /// <summary>
  /// Access to operations
  /// </summary>
  public IDbOperations Operations { get; internal set; }

  /// <summary>
  /// Parameters from the interceptor which ran on before the operation (only available for completed operations)
  /// </summary>
  public Dictionary<string, object> Properties { get; set; } = new();

  /// <summary>
  /// Get a typed property
  /// </summary>
  public TProp Property<TProp>(string key) => Properties.GetValueOrDefault<TProp>(key);

  /// <summary>
  /// Get a typed property
  /// </summary>
  public bool TryGetProperty<TProp>(string key, out TProp property) => Properties.TryGetValue(key, out property);

  /// <summary>
  /// Holds a reference to the previously existing model when an update happens (can be null)
  /// </summary>
  public object PreviousModel { get; set; }
}