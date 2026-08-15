using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite.Sqlite;

namespace Mixtape.Sqlite;

public class SqliteOptions
{
  public string ConnectionString { get; set; }

  public Action<IDbConnection> OnConnectionCreate { get; set; }

  public Action<SqliteOrmLiteDialectProviderBase> OnConfigure { get; set; }

  public List<Type> RegisteredTables { get; private set; } = [];
}