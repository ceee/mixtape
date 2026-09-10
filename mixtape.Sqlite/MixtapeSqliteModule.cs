using System;
using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Mixtape.Configuration;
using Mixtape.Extensions;
using Mixtape.Identity;
using Mixtape.Media;
using Mixtape.Models;
using Mixtape.Modules;
using Mixtape.Numbers;
using Mixtape.Sqlite.Migrations;
using Mixtape.Tokens;
using ServiceStack;
using ServiceStack.Logging;
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
    //services.AddOrmLite(options => options.UseSqlite(connectionString));
    
    services.AddSingleton<IDbConnectionFactory>(CreateDbConnectionFactory);
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

  public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
  {
    RunMigrations(serviceProvider);
  }


  protected IDbConnectionFactory CreateDbConnectionFactory(IServiceProvider services)
  {
    IMixtapeOptions options = services.GetService<IMixtapeOptions>();
    SqliteOptions sqliteOptions = options.For<SqliteOptions>();
    
    LogManager.LogFactory = new NetCoreLogFactory(services.GetService<ILoggerFactory>());
    
    SqliteOrmLiteDialectProviderBase dialect = SqliteDialect.Create();
    //dialect.UseJson = true;
    //dialect.UseUtc = true;
    dialect.EnableWal = true;
    dialect.EnableForeignKeys = true;
    dialect.BusyTimeout = TimeSpan.FromSeconds(30);

    sqliteOptions.OnConfigure?.Invoke(dialect);
    
    return new OrmLiteConnectionFactory(sqliteOptions.ConnectionString, dialect);
  }


  /// <summary>
  /// Run migrations from entry assembly
  /// </summary>
  protected void RunMigrations(IServiceProvider services)
  {
    IDbConnectionFactory factory = services.GetService<IDbConnectionFactory>();
    Assembly assembly = Assembly.GetEntryAssembly();

    if (assembly == null)
    {
      return;
    }

    MixtapeSqliteMigrator migrator = new(factory, LogManager.LogFactory, assembly);
    migrator.Run();
  }
}