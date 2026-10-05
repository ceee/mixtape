namespace Mixtape.Sqlite;

public class Interceptors(IMixtapeContext context, IMixtapeStore store, Lazy<IEnumerable<IInterceptor>> registrations, ILogger<IInterceptor> logger)
  : IInterceptors
{
  protected IMixtapeContext Context { get; set; } = context;

  protected IMixtapeStore Store { get; set; } = store;

  protected Lazy<IEnumerable<IInterceptor>> Registrations { get; set; } = registrations;

  protected ILogger<IInterceptor> Logger { get; set; } = logger;


  /// <inheritdoc />
  public InterceptorInstruction<T> ForCreate<T>(T model) where T : MixtapeIdEntity, new() => new(this, Store, Context, Registrations, Logger, InterceptorRunType.Create, model);

  /// <inheritdoc />
  public InterceptorInstruction<T> ForUpdate<T>(T model, T previousModel = null) where T : MixtapeIdEntity, new() => new(this, Store, Context, Registrations, Logger, InterceptorRunType.Update, model, previousModel);

  /// <inheritdoc />
  public InterceptorInstruction<T> ForDelete<T>(T model) where T : MixtapeIdEntity, new() => new(this, Store, Context, Registrations, Logger, InterceptorRunType.Delete, model);
}


public interface IInterceptors
{
  /// <summary>
  /// Instruction which can run interceptors before and after a creating an entity
  /// </summary>
  InterceptorInstruction<T> ForCreate<T>(T model) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Instruction which can run interceptors before and after updating an entity
  /// </summary>
  InterceptorInstruction<T> ForUpdate<T>(T model, T previousModel = null) where T : MixtapeIdEntity, new();

  /// <summary>
  /// Instruction which can run interceptors before and after deleting an entity
  /// </summary>
  InterceptorInstruction<T> ForDelete<T>(T model) where T : MixtapeIdEntity, new();
}