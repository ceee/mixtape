using System;
using System.Collections.Generic;
using Fisher;

namespace Mixtape.Sqlite;

public class SqliteOptions
{
  public string ConnectionString { get; set; }

  public Action<StoreOptions> OnConfigure { get; set; }

  public List<Type> RegisteredTables { get; private set; } = [];
}