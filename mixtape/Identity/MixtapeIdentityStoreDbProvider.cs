using System.Linq.Expressions;

namespace Mixtape.Identity;

public interface IMixtapeIdentityStoreDbProvider
{
  Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeEntity, new();

  Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new();

  Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
}