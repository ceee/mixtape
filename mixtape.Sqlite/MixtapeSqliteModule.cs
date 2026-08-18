using Fisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mixtape.Extensions;
using Mixtape.Identity;
using Mixtape.Media;
using Mixtape.Models;
using Mixtape.Modules;
using Mixtape.Numbers;
using Mixtape.Tokens;

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
    services.AddOptions<FlavorOptions>();
    services.AddOptions<SqliteOptions>().Bind(configuration.GetSection("Mixtape:Sqlite"));
    services.ConfigureOptions<ConfigureFlavorJsonOptions>();
    
    services.AddFisher(svc =>
    {
      SqliteOptions config = svc.GetRequiredService<IOptions<SqliteOptions>>().Value;

      StoreOptions options = new();
      options.Connection($"Data Source={config.ConnectionString}");
        config.OnConfigure?.Invoke(options);
        return options;
      })
      .ApplyAllDatabaseChangesOnStartup()
      .SeedInitialDataOnStartup();
    
    services.AddScoped<IMixtapeStore, MixtapeStore>();
    services.AddScoped<IDbOperations, DbOperations>();
    services.AddScoped<StoreContext>();
    services.AddScoped<IEntityModifiedHandler, EmptyEntityModifiedHandler>();
    
    services.Replace<IMixtapeIdentityStoreDbProvider, SqliteIdentityStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeMediaStoreDbProvider, SqliteMediaStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeNumberStoreDbProvider, SqliteNumberStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeTokenStoreDbProvider, SqliteTokenStoreDbProvider>(ServiceLifetime.Scoped);
  }
}