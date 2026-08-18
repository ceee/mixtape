using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fisher.Linq;
using Fisher.Pagination;
using Mixtape.Extensions;
using Mixtape.Models;
using Mixtape.Utils;

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

    return WhenActive(await Session.LoadAsync<T>(id));
  }


  /// <inheritdoc />
  public virtual async Task<Dictionary<string, T>> Load<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new()
  {
    ids = ids.Distinct().ToArray();

    IReadOnlyList<T> models = await Session.LoadManyAsync<T>(ids.ToArray());
    Dictionary<string, T> result = new();

    foreach (string id in ids)
    {
      T entity = models.FirstOrDefault(x => x.Id == id);
      result.Add(id, WhenActive(entity));
    }

    return result;
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> LoadAsList<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new()
  {
    ids = ids.Distinct().ToArray();

    IReadOnlyList<T> models = await Session.LoadManyAsync<T>(ids.ToArray());
    return models.ToList();
  }


  /// <inheritdoc />
  public virtual async Task<bool> Any<T>(Func<IQueryable<T>, IQueryable<T>> querySelector = default) where T : MixtapeIdEntity, new()
  {
    querySelector ??= x => x;
    return await querySelector(Session.Query<T>()).AnyAsync();
  }


  /// <inheritdoc />
  public virtual async Task<Paged<T>> Load<T>(int pageNumber, int pageSize, Func<IQueryable<T>, IQueryable<T>> querySelector = default) where T : MixtapeIdEntity, new()
  {
    IQueryable<T> queryable = Session.Query<T>();
    querySelector ??= x => x;

    IPagedList<T> result = await querySelector(queryable).ToPagedListAsync(pageNumber, pageSize);
    return new((List<T>)result, result.TotalItemCount, pageNumber, pageSize);
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> Load<T>(Func<IQueryable<T>, IQueryable<T>> querySelector) where T : MixtapeIdEntity, new()
  {
    IQueryable<T> queryable = Session.Query<T>();
    querySelector ??= x => x;

    return (List<T>)(await querySelector(queryable).ToListAsync());
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> Load<T>(Expression<Func<T, bool>> predicate) where T : MixtapeIdEntity, new()
  {
    return (List<T>)(await Session.Query<T>().Where(predicate).ToListAsync());
  }


  /// <inheritdoc />
  public virtual async Task<List<T>> LoadAll<T>() where T : MixtapeIdEntity, new()
  {
    return (List<T>)(await Session.Query<T>().ToListAsync());
  }
}