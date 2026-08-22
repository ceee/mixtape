using Microsoft.Extensions.Logging;

namespace Mixtape.Raven;

public class InterceptorInstruction<T> where T : MixtapeIdEntity, new()
{
  public Guid Guid { get; private set; }

  public InterceptorRunType Runtype { get; }

  public T Model { get; }

  public T PreviousModel { get; }

  public Type ModelType { get; }

  public Result<T> Result { get; private set; }

  protected IMixtapeContext Context { get; }
  
  protected IMixtapeStore Store { get; }

  protected Lazy<IEnumerable<IInterceptor>> Interceptors { get; }

  protected Dictionary<IInterceptor, InterceptorParameters> InterceptorCache { get; } = new();

  protected IInterceptors InterceptorHandler { get; }

  protected ILogger<IInterceptor> Logger { get; }

  protected Func<IInterceptor, bool> InterceptorFilter { get; private set; } = x => true;


  internal InterceptorInstruction(IInterceptors interceptors, IMixtapeStore store, IMixtapeContext context, Lazy<IEnumerable<IInterceptor>> registrations, ILogger<IInterceptor> logger, InterceptorRunType runtype, T model, T previousModel = null)
  {
    InterceptorHandler = interceptors;
    Store = store;
    Context = context;
    Guid = Guid.NewGuid();
    Logger = logger;
    Runtype = runtype;
    Model = model;
    PreviousModel = previousModel;
    Interceptors = registrations;

    ModelType = model.GetType();
  }


  /// <summary>
  /// Custom interceptor filter for this instruction (the CanHandle() method for each interceptor is still activated)
  /// </summary>
  public void Filter(Func<IInterceptor, bool> predicate)
  {
    InterceptorFilter = predicate;
  }


  /// <summary>
  /// Run all interceptors (in order) which can handle the given type.
  /// Depending on the action any of the following methods on the interceptor is called: Creating(), Updating(), Deleting().
  /// If any of the interceptors returns a result the operation is cancelled and this result is returned.
  /// </summary>
  public async Task<bool> Start(IRavenOperations operations)
  {
    foreach (IInterceptor interceptor in GetInterceptors())
    {
      InterceptorParameters parameters = new()
      {
        Context = Context,
        Store =  Store,
        Properties = new(),
        Interceptors = InterceptorHandler,
        Operations = operations,
        PreviousModel = PreviousModel
      };

      if (!interceptor.CanHandle(parameters, ModelType))
      {
        continue;
      }
    

      Logger.LogDebug("Run interceptor {interceptor} for {type}:{operation}", interceptor.Name, ModelType, Runtype);

      InterceptorResult<MixtapeIdEntity> result = (await HandleBefore(interceptor, parameters)) ?? new();
      result.InterceptorHash = Hashimoto.Sha1();

      InterceptorCache.Add(interceptor, parameters);

      // we cancel all further interceptors if a result is available and return this instead
      if (result.Result != null)
      {
        Result = result.Result.ConvertTo(result.Result.Model as T);
        return false;
      }

      // the Continue task will cancel all further interceptors
      if (!result.Continue)
      {
        break;
      }
    }

    return true;
  }


  /// <summary>
  /// Run all interceptors (in order) which can handle the given type.
  /// Depending on the action any of the following methods on the interceptor is called: Created(), Updated(), Deleted().
  /// The parameters which are returned from the Start() operation are passed to the methods.
  /// </summary>
  public async Task Complete()
  {
    foreach ((IInterceptor interceptor, InterceptorParameters parameters) in InterceptorCache)
    {
      await HandleAfter(interceptor, parameters);
    }
  }


  protected IEnumerable<IInterceptor> GetInterceptors()
  {
    return Interceptors.Value.Where(InterceptorFilter).OrderByDescending(x => x.Gravity);
  }


  /// <summary>
  /// Proxy for handling methods on an interceptor
  /// </summary>
  protected Task<InterceptorResult<MixtapeIdEntity>> HandleBefore(IInterceptor interceptor, InterceptorParameters parameters) => Runtype switch
  {
    InterceptorRunType.Create => interceptor.Creating(parameters, Model),
    InterceptorRunType.Update => interceptor.Updating(parameters, Model),
    InterceptorRunType.Delete => interceptor.Deleting(parameters, Model),
    _ => throw new NotImplementedException()
  };


  /// <summary>
  /// Proxy for handling methods on an interceptor
  /// </summary>
  protected Task HandleAfter(IInterceptor interceptor, InterceptorParameters parameters) => Runtype switch
  {
    InterceptorRunType.Create => interceptor.Created(parameters, Model),
    InterceptorRunType.Update => interceptor.Updated(parameters, Model),
    InterceptorRunType.Delete => interceptor.Deleted(parameters, Model),
    _ => throw new NotImplementedException()
  };
}