namespace Mixtape.Tokens;

public interface IMixtapeTokenStoreDbProvider
{
  Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeIdEntity, new();

  Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new();

  Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new();
}


public class EmptyMixtapeTokenStoreDbProvider : IMixtapeTokenStoreDbProvider
{
    public Task<Result<T>> Create<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new()
    {
        throw new NotImplementedException();
    }

    public Task<Result<T>> Delete<T>(T model, CancellationToken ct = default) where T : MixtapeIdEntity, new()
    {
        throw new NotImplementedException();
    }
    
    public Task<T> Load<T>(string id, CancellationToken ct = default) where T : MixtapeIdEntity, new()
    {
        throw new NotImplementedException();
    }
}