using System.Threading.Tasks;
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
      return Result<T>.Fail("@errors.ondelete.idnotfound");
    }

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
      Session.Delete(model);
    }

    await Session.SaveChangesAsync();
    // if (InterceptorBlocker == null)
    // {
    //   await instruction.Complete();
    //   await Session.SaveChangesAsync();
    // }

    return Result<T>.Success();
  }


  /// <inheritdoc />
  // public virtual async Task Purge<T>(string querySuffix = null, Parameters parameters = null) where T : MixtapeIdEntity, new()
  // {
  //   var collectionName = Store.Raven.Conventions.FindCollectionName(typeof(T));
  //   var operationQuery = new DeleteByQueryOperation(new IndexQuery()
  //   {
  //     Query = $"from {collectionName} c {querySuffix ?? string.Empty}",
  //     QueryParameters = parameters
  //   }, new QueryOperationOptions { AllowStale = true });
  //
  //   Operation operation = await Store.Raven.Operations.SendAsync(operationQuery);
  //   await operation.WaitForCompletionAsync();
  // }
}