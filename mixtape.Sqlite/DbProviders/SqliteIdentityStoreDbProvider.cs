using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fisher.Linq;
using Mixtape.Identity;
using Mixtape.Models;

namespace Mixtape.Sqlite;

public class SqliteIdentityStoreDbProvider(IDbOperations ops) : IMixtapeIdentityStoreDbProvider
{
  public Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    ops.Load<T>(id);


  public Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    ops.Session.Query<T>().FirstOrDefaultAsync(expression, ct);
  

  public async Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    await ops.Load(expression);

  
  public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    ops.Create(model);


  public Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    ops.Update(model);


  public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    ops.Delete(model);
}