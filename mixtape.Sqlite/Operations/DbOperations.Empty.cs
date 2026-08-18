using System.Threading.Tasks;
using Mixtape.Models;

namespace Mixtape.Sqlite;

public partial class DbOperations : IDbOperations
{
  /// <inheritdoc />
  public virtual Task<T> Empty<T>(string flavorAlias = null) where T : MixtapeIdEntity, ISupportsFlavors, new() => Empty<T, T>(flavorAlias);


  /// <inheritdoc />
  public virtual Task<TFlavor> Empty<T, TFlavor>(string flavorAlias = null)
    where T : MixtapeIdEntity, ISupportsFlavors, new()
    where TFlavor : T, new()
  {
    return Task.FromResult(Flavors.Construct<T, TFlavor>(flavorAlias));
  }
}