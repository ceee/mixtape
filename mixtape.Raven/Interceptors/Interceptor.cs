namespace Mixtape.Raven;


public abstract partial class Interceptor<T> : Interceptor, IInterceptor<T> where T : MixtapeIdEntity
{
  /// <inheritdoc />
  public Type ModelType { get; protected set; }


  public Interceptor()
  {
    Gravity = 0;
    ModelType = typeof(T);
    Name = ModelType.Name;
  }


  /// <inheritdoc />
  public override bool CanHandle(InterceptorParameters args, Type modelType) => ModelType.IsAssignableFrom(modelType);

  /// <inheritdoc />
  public virtual Task<InterceptorResult<T>> Creating(InterceptorParameters args, T model) => Task.FromResult<InterceptorResult<T>>(default);

  /// <inheritdoc />
  public sealed override async Task<InterceptorResult<MixtapeIdEntity>> Creating(InterceptorParameters args, MixtapeIdEntity model) => Convert(await Creating(args, model as T));

  /// <inheritdoc />
  public virtual Task<InterceptorResult<T>> Updating(InterceptorParameters args, T model) => Task.FromResult<InterceptorResult<T>>(default);

  /// <inheritdoc />
  public sealed override async Task<InterceptorResult<MixtapeIdEntity>> Updating(InterceptorParameters args, MixtapeIdEntity model) => Convert(await Updating(args, model as T));

  /// <inheritdoc />
  public virtual Task<InterceptorResult<T>> Deleting(InterceptorParameters args, T model) => Task.FromResult<InterceptorResult<T>>(default);

  /// <inheritdoc />
  public sealed override async Task<InterceptorResult<MixtapeIdEntity>> Deleting(InterceptorParameters args, MixtapeIdEntity model) => Convert(await Deleting(args, model as T));

  /// <inheritdoc />
  public virtual Task Created(InterceptorParameters args, T model) => Task.CompletedTask;

  /// <inheritdoc />
  public sealed override Task Created(InterceptorParameters args, MixtapeIdEntity model) => Created(args, model as T);

  /// <inheritdoc />
  public virtual Task Updated(InterceptorParameters args, T model) => Task.CompletedTask;

  /// <inheritdoc />
  public sealed override Task Updated(InterceptorParameters args, MixtapeIdEntity model) => Updated(args, model as T);

  /// <inheritdoc />
  public virtual Task Deleted(InterceptorParameters args, T model) => Task.CompletedTask;

  /// <inheritdoc />
  public sealed override Task Deleted(InterceptorParameters args, MixtapeIdEntity model) => Deleted(args, model as T);


  InterceptorResult<MixtapeIdEntity> Convert(InterceptorResult<T> result)
  {
    if (result == default)
    {
      return default;
    }

    return new InterceptorResult<MixtapeIdEntity>()
    {
      Continue = result.Continue,
      InterceptorHash = result.InterceptorHash,
      Result = result.Result != null ? result.Result.ConvertTo<MixtapeIdEntity>(result.Result.Model) : null
    };
  }
}


public abstract partial class Interceptor : IInterceptor
{
  /// <inheritdoc />
  public int Gravity { get; protected set; }

  /// <inheritdoc />
  public string Name { get; protected set; }

  /// <inheritdoc />
  public string Hash { get; protected set; }


  public Interceptor()
  {
    Gravity = 0;
    Hash = Hashimoto.Sha1();
  }


  /// <inheritdoc />
  public virtual bool CanHandle(InterceptorParameters args, Type modelType) => true;

  /// <inheritdoc />
  public virtual Task<InterceptorResult<MixtapeIdEntity>> Creating(InterceptorParameters args, MixtapeIdEntity model) => Task.FromResult<InterceptorResult<MixtapeIdEntity>>(default);

  /// <inheritdoc />
  public virtual Task<InterceptorResult<MixtapeIdEntity>> Updating(InterceptorParameters args, MixtapeIdEntity model) => Task.FromResult<InterceptorResult<MixtapeIdEntity>>(default);

  /// <inheritdoc />
  public virtual Task<InterceptorResult<MixtapeIdEntity>> Deleting(InterceptorParameters args, MixtapeIdEntity model) => Task.FromResult<InterceptorResult<MixtapeIdEntity>>(default);

  /// <inheritdoc />
  public virtual Task Created(InterceptorParameters args, MixtapeIdEntity model) => Task.CompletedTask;

  /// <inheritdoc />
  public virtual Task Updated(InterceptorParameters args, MixtapeIdEntity model) => Task.CompletedTask;

  /// <inheritdoc />
  public virtual Task Deleted(InterceptorParameters args, MixtapeIdEntity model) => Task.CompletedTask;
}


public interface IInterceptor<T> : IInterceptor where T : MixtapeIdEntity
{
  /// <summary>
  /// Type of the associated model
  /// </summary>
  Type ModelType { get; }

  /// <summary>
  /// Called after an entity has been stored but before the session has saved its changes
  /// </summary>
  Task Created(InterceptorParameters args, T model);

  /// <summary>
  /// Called before an entity is stored and validated
  /// </summary>
  Task<InterceptorResult<T>> Creating(InterceptorParameters args, T model);

  /// <summary>
  /// Called after an entity has been deleted but before the session has saved its changes
  /// </summary>
  Task Deleted(InterceptorParameters args, T model);

  /// <summary>
  /// Called before an entity is deleted
  /// </summary>
  Task<InterceptorResult<T>> Deleting(InterceptorParameters args, T model);

  /// <summary>
  /// Called after an entity has been updated but before the session has saved its changes
  /// </summary>
  Task Updated(InterceptorParameters args, T model);

  /// <summary>
  /// Called before an entity is stored and validated
  /// </summary>
  Task<InterceptorResult<T>> Updating(InterceptorParameters args, T model);
}


public interface IInterceptor
{
  /// <summary>
  /// Interceptors with a higher gravity will run before any with lower gravity
  /// </summary>
  int Gravity { get; }

  /// <summary>
  /// Readable name for this interceptor
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Hash which is used for this interceptor to be able to pass results from Start to Complete methods
  /// </summary>
  string Hash { get; }

  /// <summary>
  /// Whether any of the interceptor methods is allowed to run based on the parameters
  /// </summary>
  bool CanHandle(InterceptorParameters args, Type modelType);

  /// <summary>
  /// Called after an entity has been stored but before the session has saved its changes
  /// </summary>
  Task Created(InterceptorParameters args, MixtapeIdEntity model);

  /// <summary>
  /// Called before an entity is stored and validated
  /// </summary>
  Task<InterceptorResult<MixtapeIdEntity>> Creating(InterceptorParameters args, MixtapeIdEntity model);

  /// <summary>
  /// Called after an entity has been deleted but before the session has saved its changes
  /// </summary>
  Task Deleted(InterceptorParameters args, MixtapeIdEntity model);

  /// <summary>
  /// Called before an entity is deleted
  /// </summary>
  Task<InterceptorResult<MixtapeIdEntity>> Deleting(InterceptorParameters args, MixtapeIdEntity model);

  /// <summary>
  /// Called after an entity has been updated but before the session has saved its changes
  /// </summary>
  Task Updated(InterceptorParameters args, MixtapeIdEntity model);

  /// <summary>
  /// Called before an entity is stored and validated
  /// </summary>
  Task<InterceptorResult<MixtapeIdEntity>> Updating(InterceptorParameters args, MixtapeIdEntity model);
}