using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;

namespace Mixtape.Raven;

public partial class RavenOperations : IRavenOperations
{
  /// <inheritdoc />
  public virtual Task<bool> IsAllowedAsChild<T>(T model, string parentId) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    return Task.FromResult(true);
  }


  /// <inheritdoc />
  public async Task<T[]> GetHierarchy<T, TIndex>(string id) 
    where T : MixtapeIdEntity, ISupportsTrees, new() 
    where TIndex : MixtapeTreeHierarchyIndex<T>, new()
  {
    MixtapeTreeHierarchyIndexResult result = await Session.Query<MixtapeTreeHierarchyIndexResult, TIndex>()
      .ProjectInto<MixtapeTreeHierarchyIndexResult>()
      .Include<MixtapeTreeHierarchyIndexResult, T>(x => x.Path)
      .Include<MixtapeTreeHierarchyIndexResult, T>(x => x.Id)
      .FirstOrDefaultAsync(x => x.Id == id);

    if (result == null)
    {
      return [];
    }

    List<string> ids = result.Path ?? [];
    ids.Add(id);

    return (await Session.LoadAsync<T>(ids)).Select(x => x.Value).ToArray();
  }


  /// <inheritdoc />
  public async Task<Paged<T>> LoadChildren<T>(string parentId, int pageNumber, int pageSize, Func<IRavenQueryable<T>, IQueryable<T>> querySelector = default) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    IRavenQueryable<T> queryable = Session.Query<T>().Where(x => x.ParentId == parentId).Statistics(out QueryStatistics statistics);
    querySelector ??= x => x.OrderBy(x => x.Sort);

    List<T> result = await querySelector(queryable).Paging(pageNumber, pageSize).ToListAsync();
    return new Paged<T>(result, statistics.TotalResults, pageNumber, pageSize);
  }


  /// <inheritdoc />
  public async Task<Paged<T>> LoadChildren<T, TIndex>(string parentId, int pageNumber, int pageSize, Func<IRavenQueryable<T>, IQueryable<T>> querySelector = default) 
    where T : MixtapeIdEntity, ISupportsTrees, new() 
    where TIndex : AbstractCommonApiForIndexes, new()
  {
    IRavenQueryable<T> queryable = Session.Query<T, TIndex>().Where(x => x.ParentId == parentId).Statistics(out QueryStatistics statistics);
    querySelector ??= x => x.OrderBy(x => x.Sort);

    var query = querySelector(queryable).Paging(pageNumber, pageSize);

    List<T> result = await query.ToListAsync();
    return new Paged<T>(result, statistics.TotalResults, pageNumber, pageSize);
  }


  /// <inheritdoc />
  public async Task<Result<T>> Move<T>(string id, string newParentId, Func<T, string, Task<bool>> isParentAllowed = null) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    T model = await Load<T>(id);
    T parent = await Load<T>(newParentId);

    if (model == null || (!newParentId.IsNullOrEmpty() && parent == null))
    {
      return Result<T>.Fail("@errors.idnotfound");
    }

    if (isParentAllowed != null && !await isParentAllowed(model, newParentId))
    {
      return Result<T>.Fail("@errors.treeentity.parentnotallowed");
    }

    model.ParentId = parent?.Id;

    return await Update(model);
  }


  /// <inheritdoc />
  public Task<Result<T>> Copy<T>(string id, string newParentId, Func<T, string, Task<bool>> isParentAllowed = null) where T : MixtapeIdEntity, ISupportsTrees, new() => Copy<T>(id, newParentId, false, isParentAllowed);


  /// <inheritdoc />
  public Task<Result<T>> CopyWithDescendants<T>(string id, string newParentId, Func<T, string, Task<bool>> isParentAllowed = null) where T : MixtapeIdEntity, ISupportsTrees, new() => Copy<T>(id, newParentId, true, isParentAllowed);


  /// <summary>
  /// Copies an entity (with optional descendants) to a new location
  /// </summary>
  protected async Task<Result<T>> Copy<T>(string id, string newParentId, bool includeDescendants, Func<T, string, Task<bool>> isParentAllowed = null, bool isDescendant = false) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    T originalModel = await Load<T>(id);
    T model = ObjectCopycat.Clone(originalModel);
    T parent = await Load<T>(newParentId);

    if (model == null || (!newParentId.IsNullOrEmpty() && parent == null))
    {
      return Result<T>.Fail("@errors.idnotfound");
    }

    string baseId = model.Id;
    string originalParentId = model.ParentId;

    // update new page properties
    model.Id = null;
    model.ParentId = parent?.Id;

    if (model is MixtapeEntity mixtapeEntity)
    {
      mixtapeEntity.IsActive = !isDescendant ? false : (originalModel as MixtapeEntity).IsActive;
      mixtapeEntity.CreatedDate = DateTime.Now;
      mixtapeEntity.Hash = null;
    }

    // check if new parent is allowed
    if (isParentAllowed != null && !await isParentAllowed(model, newParentId))
    {
      return Result<T>.Fail("@errors.treeentity.parentnotallowed");
    }

    Result<T> result = await Create(model);

    // recursive function to store descendants
    if (includeDescendants)
    {
      List<T> children = await Session.Query<T>().Where(x => x.ParentId == baseId).ToListAsync();

      foreach (T child in children)
      {
        await Copy(child.Id, model.Id, true, isParentAllowed, true);
      }
    }

    return result;
  }


  /// <inheritdoc />
  public async Task<Result<string[]>> DeleteWithDescendants<T>(T model) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    List<T> pages = await GetDescendantsAndSelf(model);

    if (pages.Count < 1)
    {
      return Result<string[]>.Fail("@errors.ondelete.idnotfound");
    }

    await this.Delete(pages.ToArray());
    return Result<string[]>.Success(pages.Select(x => x.Id).ToArray());
  }


  /// <summary>
  /// Get an entity with all its descendants
  /// </summary>
  async Task<List<T>> GetDescendantsAndSelf<T>(T model) where T : MixtapeIdEntity, ISupportsTrees, new()
  {
    List<T> items = [model];

    async Task AddChildren(T parent)
    {
      string parentId = parent.Id;
      List<T> children = await Session.Query<T>().Where(x => x.ParentId == parentId).ToListAsync();
      items.AddRange(children);

      foreach (T child in children)
      {
        await AddChildren(child);
      }
    }

    await AddChildren(model);

    return items;
  }
}