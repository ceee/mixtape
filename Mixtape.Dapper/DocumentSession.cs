using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Mixtape.Sqlite;

public class DocumentSession(IOptions<SqliteOptions> sqliteOptions)
  : SqliteConnection(sqliteOptions.Value.ConnectionString), IDocumentSession
{
  /// <inheritdoc />
  public IDocumentSession New()
  {
    return new DocumentSession(sqliteOptions);
  }
}

public interface IDocumentSession : IDbConnection, IAsyncDisposable
{
  /// <summary>
  /// Creates a new, fresh session
  /// </summary>
  IDocumentSession New();
}