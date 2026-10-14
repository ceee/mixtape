using System.Linq.Expressions;
using Dommel;

namespace Mixtape.Sqlite;

public partial class DbOperations : IDbOperations
{
  /// <inheritdoc />
  public virtual async Task<T> Load<T>(string id, string changeVector = null) where T : MixtapeIdEntity, new()
  {
    if (id.IsNullOrWhiteSpace())
    {
      return null;
    }
    if (!changeVector.IsNullOrEmpty())
    {
      //return WhenActive(await GetRevision(changeVector)); // TODO
    }

    return WhenActive(await Session.GetAsync<T>(id));
  }


  /// <inheritdoc />
  public virtual async Task<Dictionary<string, T>> Load<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new()
  {
    string[] idArr = [.. ids.Distinct()];

    Dictionary<string, T> models = (await Session.SelectAsync<T>(x => idArr.Contains(x.Id))).ToDictionary(x => x.Id, x => x);
    Dictionary<string, T> result = new();

    foreach (string id in idArr)
    {
      T entity = models.GetValueOrDefault(id);
      result.Add(id, WhenActive(entity));
    }

    return result;
  }


  /// <inheritdoc />
  public virtual async Task<T> Find<T>(Expression<Func<T, bool>> predicate) where T : MixtapeIdEntity, new()
  {
    return await Session.GetAsync<T>(predicate);
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> LoadAsList<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new()
  {
    return [.. await Session.SelectAsync<T>(x => ids.Contains(x.Id))];
  }


  /// <inheritdoc />
  public virtual async Task<bool> Any<T>(Expression<Func<T, bool>> predicate = null) where T : MixtapeIdEntity, new()
  {
    predicate ??= x => true;
    return await Session.CountAsync(predicate) > 0;
  }


  /// <inheritdoc />
  public virtual async Task<Paged<T>> Load<T>(int pageNumber, int pageSize, Expression<Func<T, bool>> predicate = null) where T : MixtapeIdEntity, new()
  {
    predicate ??= x => true;

    List<T> result = [.. await Session.SelectPagedAsync<T>(predicate, pageNumber, pageSize)];
    long totalCount = await Session.CountAsync<T>(predicate);

    return new(result, totalCount, pageNumber, pageSize);
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> Load<T>(Expression<Func<T, bool>> predicate) where T : MixtapeIdEntity, new()
  {
    predicate ??= x => true;
    return [.. await Session.SelectAsync<T>(predicate)];
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> LoadAll<T>() where T : MixtapeIdEntity, new()
  {
    return [..await Session.GetAllAsync<T>()];
  }
}