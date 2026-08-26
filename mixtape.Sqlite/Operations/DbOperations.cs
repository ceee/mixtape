using System;
using System.Collections.Generic;
using System.Data;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;
using Mixtape.Communication;
using Mixtape.Context;
using Mixtape.Models;
using Mixtape.Utils;
using Mixtape.Validation;

namespace Mixtape.Sqlite;

public partial class DbOperations : IDbOperations
{
  protected IMixtapeContext Context { get; private set; }

  protected FlavorOptions Flavors { get; }

  protected IServiceProvider Services { get; }

  public IDbConnection Db { get; }

  protected ILogger<IDbOperations> Logger { get; }

  protected IEntityModifiedHandler EntityModifiedHandler { get; }

  
  public DbOperations(StoreContext context, IDbConnection db, ILogger<IDbOperations> logger, IHandlerHolder handler)
  {
    Context = context.Context;
    Services = context.Services;
    Flavors = context.Options.For<FlavorOptions>();
    Db = db;
    Logger = logger;
    EntityModifiedHandler = handler.Get<IEntityModifiedHandler>();
  }


  /// <inheritdoc />
  public bool EnsureTableExists<T>() where T : MixtapeIdEntity
  {
    return Db.CreateTableIfNotExists<T>();
  }


  /// <inheritdoc />
  public Task<string> GenerateId<T>(T model) where T : MixtapeIdEntity
  {
    return Task.FromResult(IdGenerator.Create(12));
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

    if (model is not MixtapeEntity mixtapeModel)
    {
      return model;
    }

    // set default properties
    if (mixtapeModel.CreatedDate == default)
    {
      mixtapeModel.CreatedDate = DateTimeOffset.Now;
    }

    // update name alias and last modified
    mixtapeModel.Alias = Safenames.Alias(mixtapeModel.Name);
    mixtapeModel.LastModifiedDate = DateTimeOffset.Now;
    mixtapeModel.Hash ??= Hashimoto.Sha1()[..12];

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
  public virtual T WhenActive<T>(T model) where T : MixtapeIdEntity, new()
  {
    return model; // TODO should we really use this? I tried to get data in a custom backend but couldn't because of this
    //return model != null && (model is not MixtapeEntity || (model as MixtapeEntity).IsActive) && (model is not ISupportsSoftDelete || !(model as ISupportsSoftDelete).IsDeleted) ? model : default;
  }
  
  /// <summary>
  /// Get log level for an entity when modifying data in db.
  /// Defaults to LogLevel.Information
  /// </summary>
  protected virtual LogLevel GetLogLevel<T>(T model = null) where T : MixtapeIdEntity, new()
  {
    SqliteLoggingAttribute attribute = typeof(T).GetCustomAttribute<SqliteLoggingAttribute>(true);
    return attribute?.LogLevel ?? LogLevel.Information;
  }
}


public interface IDbOperations
{
  IDbConnection Db { get; }
  
  /// <summary>
  /// Create a table if not existing
  /// </summary>
  bool EnsureTableExists<T>() where T : MixtapeIdEntity;

  /// <summary>
  /// Generate model Id by using configured document store conventions
  /// </summary>
  Task<string> GenerateId<T>(T model) where T : MixtapeIdEntity;

  /// <summary>
  /// Do only return the model when it is set to active or inactive entities are included with IncludeInactive()
  /// </summary>
  T WhenActive<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Validates an entity
  /// </summary>
  Task<ValidationResult> Validate<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Generate values for all properties (incl. nested) which contain the [GenerateId] attribute
  /// </summary>
  T AutoSetIds<T>(T model);

  /// <summary>
  /// Automatically fill base properties of a MixtapeEntity
  /// </summary>
  T PrepareForSave<T>(T model) where T : MixtapeIdEntity;

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
  /// Check if any items exist in this collection (with optional query)
  /// </summary>
  Task<bool> Any<T>(Expression<Func<T, bool>> querySelector = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by query
  /// </summary>
  Task<List<T>> Load<T>(Expression<Func<T, bool>> querySelector) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Find entity by query
  /// </summary>
  Task<T> Find<T>(Expression<Func<T, bool>> querySelector) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get entities by sql query
  /// </summary>
  Task<List<T>> LoadBySql<T>(Func<SqlExpression<T>, SqlExpression<T>> querySelector) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Get all entities from this collection. 
  /// Warning: Don't use this method for large collections. Stream the results instead.
  /// </summary>
  Task<List<T>> LoadAll<T>() where T : MixtapeIdEntity, new();

  /// <summary>
  /// Creates an entity with an optional validator
  /// </summary>
  Task<Result<T>> Create<T>(T model, Func<T, Task<ValidationResult>> validate = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Updates an entity with an optional validator
  /// </summary>
  Task<Result<T>> Update<T>(T model, Func<T, Task<ValidationResult>> validate = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Checks if an entity exists (via ID) and creates or updates it afterwards accordingly
  /// </summary>
  Task<Result<T>> CreateOrUpdate<T>(T model, Func<T, Task<ValidationResult>> validate = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Updates sorting of all items in a collection based on the given enumerable
  /// </summary>
  Task Sort<T>(IEnumerable<string> ids) where T : MixtapeEntity, new();

  /// <summary>
  /// Deletes an entity
  /// </summary>
  Task<Result<T>> Delete<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Deletes an entity
  /// </summary>
  Task<Result<T>> Delete<T>(string id) where T : MixtapeIdEntity, new();
}