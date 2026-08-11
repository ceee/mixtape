using System.Linq.Expressions;

namespace Mixtape.Media;

public interface IMixtapeMediaStoreDbProvider
{
  Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new();

  Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
  
  Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new();
}


public class EmptyMixtapeMediaStoreDbProvider : IMixtapeMediaStoreDbProvider
{
    public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new()
    {
        throw new NotImplementedException();
    }

    public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new()
    {
        throw new NotImplementedException();
    }

    public Task<T> Find<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new()
    {
        throw new NotImplementedException();
    }

    public Task<IList<T>> FindAll<T>(Expression<Func<T, bool>> expression, CancellationToken ct = default) where T : MixtapeEntity, new()
    {
        throw new NotImplementedException();
    }

    public Task<Result<T>> Update<T>(T model, CancellationToken ct = default) where T : MixtapeEntity, new()
    {
        throw new NotImplementedException();
    }
}