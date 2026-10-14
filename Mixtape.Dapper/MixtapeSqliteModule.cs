using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mixtape.Identity;
using Mixtape.Media;
using Mixtape.Numbers;
using Mixtape.Tokens;

namespace Mixtape.Sqlite;

public static class MixtapeBuilderExtensions
{
  public static MixtapeBuilder AddDapper(this MixtapeBuilder builder)
  {
    builder.AddModule<MixtapeDapperModule>();
    return builder;
  }
}

internal class MixtapeDapperModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<FlavorOptions>();
    services.AddOptions<SqliteOptions>().Bind(configuration.GetSection("Mixtape:Sqlite"));
    services.ConfigureOptions<ConfigureFlavorJsonOptions>();


    services.AddSingleton<IDbConnection>(svc => svc.GetRequiredService<SqliteConnection>());
    services.AddSingleton<DbConnection>(svc => svc.GetRequiredService<SqliteConnection>());
    services.AddSingleton<SqliteConnection>(svc =>
    {
      SqliteOptions options = svc.GetRequiredService<SqliteOptions>();
      return new SqliteConnection(options.ConnectionString);
    });

    // services.AddFisher(svc =>
    // {
    //   SqliteOptions config = svc.GetRequiredService<IOptions<SqliteOptions>>().Value;
    //
    //   StoreOptions options = new();
    //   options.Connection($"Data Source={config.ConnectionString}");
    //     config.OnConfigure?.Invoke(options);
    //     return options;
    //   })
    //   .ApplyAllDatabaseChangesOnStartup()
    //   .SeedInitialDataOnStartup();
    
    services.AddScoped<IMixtapeStore, MixtapeStore>();
    services.AddScoped<IDbOperations, DbOperations>();
    services.AddScoped<StoreContext>();
    services.AddScoped<IInterceptors, Interceptors>();
    
    services.Replace<IMixtapeIdentityStoreDbProvider, SqliteIdentityStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeMediaStoreDbProvider, SqliteMediaStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeNumberStoreDbProvider, SqliteNumberStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeTokenStoreDbProvider, SqliteTokenStoreDbProvider>(ServiceLifetime.Scoped);
  }
}