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
    await using IDocumentSession session = NewSession();
    
    if (model == null)
    {
      Logger.LogWarning("Could not delete entity (model is null) for type {type}", typeof(T));
      return Result<T>.Fail("@errors.ondelete.idnotfound");
    }

    LogLevel logLevel = GetLogLevel(model);

    // InterceptorInstruction<T> instruction = Interceptors.ForDelete(model);
    //
    // if (InterceptorBlocker == null && !await instruction.Start(this))
    // {
    //   return instruction.Result;
    // }

    if (model is ISupportsSoftDelete softDeleteModel)
    {
      softDeleteModel.IsDeleted = true;
    }
    else
    {
      session.Delete(model);
    }

    await session.SaveChangesAsync();
    // if (InterceptorBlocker == null)
    // {
    //   await instruction.Complete();
    //   await Session.SaveChangesAsync();
    // }

    Logger.Log(logLevel, "{id} ({type}) successfully deleted", typeof(T), model.Id);

    return Result<T>.Success();
  }


  /// <inheritdoc />
  public virtual async Task<int> Purge<T>() where T : MixtapeIdEntity, new()
  {
    await using IDocumentSession session = NewSession();
    int rows = await session.Query<T>().CountAsync();
    session.DeleteWhere<T>(x => true);
    return rows;
  }
}