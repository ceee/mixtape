
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;
using Mixtape.Models;

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
    
    if (model == null)
    {
      Logger.LogWarning("Could not delete entity (model is null) for type {type}", typeof(T));
      return Result<T>.Fail("@errors.ondelete.idnotfound");
    }
    
    LogLevel logLevel = GetLogLevel(model);

    if (model is ISupportsSoftDelete softDeleteModel)
    {
      softDeleteModel.IsDeleted = true;
    }
    else
    {
      await Db.DeleteByIdAsync<T>(model.Id);
    }

    Logger.Log(logLevel, "{id} ({type}) successfully deleted", typeof(T), model.Id);
    await EntityModifiedHandler.Deleted(model);

    return Result<T>.Success();
  }
}