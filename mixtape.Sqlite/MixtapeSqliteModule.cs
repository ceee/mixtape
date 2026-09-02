using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Mixtape.Configuration;
using Mixtape.Extensions;
using Mixtape.Identity;
using Mixtape.Media;
using Mixtape.Models;
using Mixtape.Modules;
using Mixtape.Numbers;
using Mixtape.Tokens;
using ServiceStack.OrmLite.Sqlite;

namespace Mixtape.Sqlite;

public static class MixtapeBuilderExtensions
{
  public static MixtapeBuilder AddSqlite(this MixtapeBuilder builder)
  {
    builder.AddModule<MixtapeSqliteModule>();
    return builder;
  }
}

internal class MixtapeSqliteModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IDbConnectionFactory>(CreateDbConnectionFactory);
    services.AddSingleton<IDbConnection>(CreateDbConnection);
    services.AddScoped<IDbOperations, DbOperations>();
    services.AddScoped<StoreContext>();
    services.AddScoped<IEntityModifiedHandler, EmptyEntityModifiedHandler>();
    services.AddOptions<FlavorOptions>();
    services.AddOptions<SqliteOptions>().Bind(configuration.GetSection("Mixtape:Sqlite"));
    services.ConfigureOptions<ConfigureFlavorJsonOptions>();
    
    services.Replace<IMixtapeIdentityStoreDbProvider, SqliteIdentityStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeMediaStoreDbProvider, SqliteMediaStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeNumberStoreDbProvider, SqliteNumberStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeTokenStoreDbProvider, SqliteTokenStoreDbProvider>(ServiceLifetime.Scoped);
  }


  protected IDbConnectionFactory CreateDbConnectionFactory(IServiceProvider services)
  {
    IMixtapeOptions options = services.GetService<IMixtapeOptions>();
    SqliteOptions sqliteOptions = options.For<SqliteOptions>();
    
    SqliteOrmLiteDialectProviderBase dialect = SqliteDialect.Create();
    //dialect.UseJson = true;
    //dialect.UseUtc = true;
    dialect.EnableWal = true;
    dialect.EnableForeignKeys = true;
    dialect.BusyTimeout = TimeSpan.FromSeconds(30);

    sqliteOptions.OnConfigure?.Invoke(dialect);
    
    return new OrmLiteConnectionFactory(sqliteOptions.ConnectionString, dialect);
  }


  protected IDbConnection CreateDbConnection(IServiceProvider services)
  {
    IDbConnectionFactory factory = services.GetService<IDbConnectionFactory>();
    IMixtapeOptions options = services.GetService<IMixtapeOptions>();
    SqliteOptions sqliteOptions = options.For<SqliteOptions>();
    IDbConnection db = factory.CreateDbConnection();
    db.Open();

    // auto create tables
    foreach (Type type in sqliteOptions.RegisteredTables)
    {
      db.CreateTableIfNotExists(type);
    }
    
    sqliteOptions.OnConnectionCreate?.Invoke(db);
    
    return db;
  }
}