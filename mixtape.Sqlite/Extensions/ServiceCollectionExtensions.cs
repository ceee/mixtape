using Microsoft.Extensions.DependencyInjection;

namespace Mixtape.Sqlite;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddSqliteTable<T>(this IServiceCollection services)
  {
    services.Configure<SqliteOptions>(opts => opts.RegisteredTables.Add(typeof(T)));
    return services;
  }
}