using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fisher;
using Mixtape.Context;
using Mixtape.Extensions;
using Mixtape.Models;
using Mixtape.Utils;
using Mixtape.Validation;

namespace Mixtape.Sqlite;

public partial class DbOperations : IDbOperations
{
  /// <inheritdoc />
  public IDocumentSession Session => Store.Session();

  protected record OperationOptions(bool IncludeInactive);

  protected IMixtapeContext Context { get; private set; }

  //protected IInterceptors Interceptors { get; }

  protected FlavorOptions Flavors { get; }

  protected IServiceProvider Services { get; }
  
  protected IMixtapeStore Store { get; }

  protected StoreInterceptorBlocker InterceptorBlocker { get; private set; }


  public DbOperations(StoreContext context)
  {
    Store = context.Store;
    Context = context.Context;
    //Interceptors =  context.Interceptors;
    Services = context.Services;
    Flavors = context.Options.For<FlavorOptions>();
  }


  /// <inheritdoc />
  public Task<string> GenerateId<T>(T model) where T : MixtapeIdEntity
  {
    IDocumentSession session = Session;
    return Task.FromResult<string>(IdGenerator.Create(16)); // TODO fisher
    //return await session.Advanced.DocumentStore.Conventions.GenerateDocumentIdAsync(session.Advanced.DocumentStore.Database, model);
  }


  /// <inheritdoc />
  public T AutoSetIds<T>(T model)
  {
    return IdGenerator.Autofill(model);
  }


  /// <inheritdoc />
  public T PrepareForSave<T>(T model) where T : MixtapeIdEntity
  {
    // set IDs
    AutoSetIds(model);

    if (model is MixtapeEntity mixtapeModel)
    {
      // get current user
      string userId = null;
      //string userId = Context.BackofficeUser.FindFirstValue(Constants.Auth.Claims.UserId).Or(Constants.Auth.SystemUser);

      // set default properties
      if (mixtapeModel.CreatedDate == default)
      {
        mixtapeModel.CreatedDate = DateTimeOffset.Now;
      }
      if (mixtapeModel.CreatedById.IsNullOrEmpty())
      {
        mixtapeModel.CreatedById = userId;
      }

      // update name alias and last modified
      mixtapeModel.Alias = Safenames.Alias(mixtapeModel.Name);
      mixtapeModel.LastModifiedById = userId;
      mixtapeModel.LastModifiedDate = DateTimeOffset.Now;
      mixtapeModel.CreatedById ??= userId;
      mixtapeModel.Hash ??= IdGenerator.Create();
    }

    return model;
  }


  /// <inheritdoc />
  public async Task<ValidationResult> Validate<T>(T model) where T : MixtapeIdEntity, new()
  {
    IMixtapeMergedValidator<T> validator = Services.GetService<IMixtapeMergedValidator<T>>();

    if (validator == null)
    {
      return new();
    }

    return await validator.ValidateAsync(model);
  }


  /// <inheritdoc />
  public StoreInterceptorBlocker WithoutInterceptors()
  {
    return InterceptorBlocker ??= new(() => InterceptorBlocker = null);
  }


  /// <inheritdoc />
  public virtual T WhenActive<T>(T model) where T : MixtapeIdEntity, new()
  {
    return model; // TODO should we really use this? I tried to get data in a custom backend but couldn't because of this
    //return model != null && (model is not MixtapeEntity || (model as MixtapeEntity).IsActive) && (model is not ISupportsSoftDelete || !(model as ISupportsSoftDelete).IsDeleted) ? model : default;
  }
}


public class StoreInterceptorBlocker : IDisposable
{
  readonly Action _onRelease;

  internal StoreInterceptorBlocker(Action onRelease)
  {
    _onRelease = onRelease;
  }

  void IDisposable.Dispose()
  {
    _onRelease();
  }
}


public interface IDbOperations
{
  /// <summary>
  /// Access to the current session
  /// </summary>
  IDocumentSession Session { get; }

  /// <summary>
  /// Get new instance of an entity (with an optional flavor)
  /// </summary>
  Task<T> Empty<T>(string flavorAlias = null) where T : MixtapeIdEntity, ISupportsFlavors, new();

  /// <summary>
  /// Get new instance of an entity with a specific flavor
  /// </summary>
  /// <param name="flavorAlias">Optional alias. If left out the default flavor is used (if configured)</param>
  Task<TFlavor> Empty<T, TFlavor>(string flavorAlias = null)
    where T : MixtapeIdEntity, ISupportsFlavors, new()
    where TFlavor : T, new();

  /// <summary>
  /// Generate model Id by using configured document store conventions
  /// </summary>
  Task<string> GenerateId<T>(T model) where T : MixtapeIdEntity;

  /// <summary>
  /// Generate values for all properties (incl. nested) which contain the [GenerateId] attribute
  /// </summary>
  T AutoSetIds<T>(T model);

  /// <summary>
  /// Automatically fill base properties of a MixtapeEntity
  /// </summary>
  T PrepareForSave<T>(T model) where T : MixtapeIdEntity;

  /// <summary>
  /// Check if any items exist in this collection (with optional query)
  /// </summary>
  Task<bool> Any<T>(Func<IQueryable<T>, IQueryable<T>> querySelector = default) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get an entity by Id
  /// </summary>
  Task<T> Load<T>(string id, string changeVector = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by ids
  /// </summary>
  Task<Dictionary<string, T>> Load<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by ids
  /// </summary>
  Task<List<T>> LoadAsList<T>(IEnumerable<string> ids) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by query
  /// </summary>
  Task<Paged<T>> Load<T>(int pageNumber, int pageSize, Func<IQueryable<T>, IQueryable<T>> expression = default) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by query
  /// </summary>
  Task<List<T>> Load<T>(Func<IQueryable<T>, IQueryable<T>> expression) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by query
  /// </summary>
  Task<List<T>> Load<T>(Expression<Func<T, bool>> predicate) where T : MixtapeIdEntity, new();
  
  /// <summary>
  /// Get all entities from this collection. 
  /// Warning: Don't use this method for large collections. Stream the results instead.
  /// </summary>
  Task<List<T>> LoadAll<T>() where T : MixtapeIdEntity, new();

  /// <summary>
  /// Validates an entity
  /// </summary>
  Task<ValidationResult> Validate<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Do not run interceptors for create/update/delete operations while this disposable is active
  /// </summary>
  StoreInterceptorBlocker WithoutInterceptors();

  /// <summary>
  /// Do only return the model when it is set to active or inactive entities are included with IncludeInactive()
  /// </summary>
  T WhenActive<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Creates an entity with an optional validator
  /// </summary>
  Task<Result<T>> Create<T>(T model, Func<T, Task<ValidationResult>> validate = null, Action<IDocumentSession> onAfterStore = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Updates an entity with an optional validator
  /// </summary>
  Task<Result<T>> Update<T>(T model, Func<T, Task<ValidationResult>> validate = null, Action<IDocumentSession> onAfterStore = null) where T : MixtapeIdEntity, new();
  
  /// <summary>
  /// Updates or creates an entity with an optional validator
  /// </summary>
  Task<Result<T>> CreateOrUpdate<T>(T model, Func<T, Task<ValidationResult>> validate = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Sort entities
  /// </summary>
  Task<Result<IOrderedEnumerable<T>>> Sort<T>(string[] sortedIds) where T : MixtapeIdEntity, ISupportsSorting, new();

  /// <summary>
  /// Batch create entities
  /// </summary>
  Task<Result<IEnumerable<T>>> CreateAll<T>(IReadOnlyCollection<T> models) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Deletes an entity
  /// </summary>
  Task<Result<T>> Delete<T>(T model) where T : MixtapeIdEntity, new();
  
  /// <summary>
  /// Deletes an entity
  /// </summary>
  Task<Result<T>> Delete<T>(string id) where T : MixtapeIdEntity, new();
}