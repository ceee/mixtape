using System;
using System.Data;
using ServiceStack.OrmLite.Sqlite;

namespace Mixtape.Sqlite;

public class SqliteOptions
{
  public string ConnectionString { get; set; }

  public Action<IDbConnection> OnConnectionCreate { get; set; }
  
  public Action<SqliteOrmLiteDialectProviderBase> OnConfigure { get; set; }
}