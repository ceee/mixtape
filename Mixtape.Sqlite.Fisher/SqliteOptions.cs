namespace Mixtape.Sqlite;

public class SqliteOptions
{
  public string ConnectionString { get; set; }

  public Action<StoreOptions> OnConfigure { get; set; }
}