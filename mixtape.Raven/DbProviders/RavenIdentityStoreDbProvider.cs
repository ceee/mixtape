using System.Linq.Expressions;
using Mixtape.Identity;

namespace Mixtape.Raven;

public class RavenIdentityStoreDbProvider(IRavenOperations ops) : IMixtapeIdentityStoreDbProvider
{
  protected IRavenOperations Ops { get; set; } = ops;


  public Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Load<T>(id);


  public Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    Ops.Session.Query<T>().FirstOrDefaultAsync(expression, ct);
  

  public async Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    await Ops.Session.Query<T>().Where(expression).ToListAsync(ct);

  
  public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Create(model);


  public Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Update(model);


  public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Delete(model);
}