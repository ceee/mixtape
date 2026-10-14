namespace Mixtape.Sqlite;

public class InterceptorResult<T>
{
  /// <summary>
  /// Autoset. Hash used to match interceptors in order to correctly pass parameters
  /// </summary>
  internal string InterceptorHash { get; set; }

  /// <summary>
  /// Prevent further interceptors from running for this operation (only valid for the current interception type, e.g. Update/Created/Purge/...)
  /// </summary>
  public bool Continue { get; set; } = true;

  /// <summary>
  /// Set a result. This will prevent further execution of the operation and cancels all subsequent interceptors
  /// </summary>
  public Result<T> Result { get; set; }
}