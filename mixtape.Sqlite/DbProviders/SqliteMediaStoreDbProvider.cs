using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Fisher.Linq;
using Mixtape.Media;
using Mixtape.Models;

namespace Mixtape.Sqlite;

public class SqliteMediaStoreDbProvider(IDbOperations ops) : IMixtapeMediaStoreDbProvider
{
  protected IDbOperations Ops { get; set; } = ops;


  public Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    Ops.Session.Query<T>().FirstOrDefaultAsync(expression, ct);
  

  public async Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) 
    where T : MixtapeEntity, new() =>
    await Ops.Load(expression);

  
  public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Create(model);


  public Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Update(model);


  public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new() =>
    Ops.Delete(model);
}