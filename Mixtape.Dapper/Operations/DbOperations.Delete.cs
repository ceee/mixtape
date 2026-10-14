using Dommel;

namespace Mixtape.Sqlite;

public partial class DbOperations : IDbOperations
{
  /// <inheritdoc />
  public virtual Task<Result<T>> Delete<T>(T model) where T : MixtapeIdEntity, new()
    => Delete<T>(model.Id);


  /// <inheritdoc />
  public virtual async Task<Result<T>> Delete<T>(string id) where T : MixtapeIdEntity, new()
  {
    T model = await Load<T>(id);
    await using IDocumentSession session = Session.New();
    
    if (model == null)
    {
      Logger.LogWarning("Could not delete entity (model is null) for type {type}", typeof(T));
      return Result<T>.Fail("@errors.ondelete.idnotfound");
    }

    LogLevel logLevel = GetLogLevel(model);

    InterceptorInstruction<T> instruction = Interceptors.ForDelete(model);

    if (InterceptorBlocker == null && !await instruction.Start(this))
    {
      return instruction.Result;
    }

    if (model is ISupportsSoftDelete softDeleteModel)
    {
      softDeleteModel.IsDeleted = true;
    }
    else
    {
      await session.DeleteAsync(model);
    }

    if (InterceptorBlocker == null)
    {
      await instruction.Complete();
    }

    Logger.Log(logLevel, "{id} ({type}) successfully deleted", typeof(T), model.Id);

    return Result<T>.Success();
  }


  /// <inheritdoc />
  public virtual async Task<int> Purge<T>() where T : MixtapeIdEntity, new()
  {
    await using IDocumentSession session = Session.New();
    long rows = await session.CountAsync<T>();
    await session.DeleteAllAsync<T>();
    return (int)rows;
  }
}