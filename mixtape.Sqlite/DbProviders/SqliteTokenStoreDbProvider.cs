using System.Threading;
using System.Threading.Tasks;
using Mixtape.Models;
using Mixtape.Tokens;

namespace Mixtape.Sqlite;

public class SqliteTokenStoreDbProvider(IDbOperations ops) : IMixtapeTokenStoreDbProvider
{
  protected IDbOperations Ops { get; set; } = ops;


  public Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeIdEntity, new() =>
    Ops.Load<T>(id);

  
  public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new() =>
    Ops.Create(model);
  

  public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new() =>
    Ops.Delete(model);
}