using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mixtape.Models;
using Mixtape.Sqlite;

namespace Mixtape.Sqlite;

public static class DbOperationsExtensions
{
  /// <summary>
  /// Deletes an entity by Id
  /// </summary>
  public static async Task<Result<T>> Delete<T>(this IDbOperations ops, string id) where T : MixtapeIdEntity, new() => await ops.Delete(await ops.Load<T>(id));

  /// <summary>
  /// Deletes entities by selector
  /// </summary>
  public static async Task<int> Delete<T>(this IDbOperations ops, Expression<Func<T, bool>> predicate) where T : MixtapeIdEntity, new() => await ops.Delete(await ops.Load<T>(predicate));

  /// <summary>
  /// Deletes entities by Id
  /// </summary>
  public static async Task<int> Delete<T>(this IDbOperations ops, IEnumerable<string> ids) where T : MixtapeIdEntity, new() => await ops.Delete((await ops.Load<T>(ids)).Select(x => x.Value));


  /// <summary>
  /// Deletes entities
  /// </summary>
  public static async Task<int> Delete<T>(this IDbOperations ops, IEnumerable<T> models) where T : MixtapeIdEntity, new()
  {
    int successCount = 0;

    foreach (T model in models)
    {
      Result<T> result = await ops.Delete(model);
      successCount += result.IsSuccess ? 1 : 0;
    }

    return successCount;
  }
}