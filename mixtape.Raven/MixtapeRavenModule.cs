using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents.Indexes;
using Raven.Client.Http;
using Mixtape.Identity;
using Mixtape.Media;
using Mixtape.Numbers;

namespace Mixtape.Raven;

public static class MixtapeBuilderExtensions
{
  public static MixtapeBuilder AddRavenDb(this MixtapeBuilder builder)
  {
    builder.AddModule<MixtapeRavenModule>();
    return builder;
  }
}

internal class MixtapeRavenModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IRavenDocumentConventionsBuilder, RavenDocumentConventionsBuilder>();
    services.AddSingleton<IDocumentStore>(CreateRavenStore);
    services.AddScoped<IMixtapeStore, MixtapeStore>();
    services.AddScoped<IMixtapeTokenProvider, MixtapeTokenProvider>();
    services.AddScoped<StoreContext>();
    services.AddTransient<IRavenOperations, RavenOperations>();
    services.AddScoped<IInterceptors, Interceptors>();

    services.Replace<IMixtapeIdentityStoreDbProvider, RavenIdentityStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeMediaStoreDbProvider, RavenMediaStoreDbProvider>(ServiceLifetime.Scoped);
    services.Replace<IMixtapeNumberStoreDbProvider, RavenNumberStoreDbProvider>(ServiceLifetime.Scoped);

    services.AddOptions<FlavorOptions>();
    services.AddOptions<RavenOptions>().Bind(configuration.GetSection("Mixtape:Raven"));
    services.ConfigureOptions<ConfigureFlavorJsonOptions>();
  }

  /// <summary>
  /// Creates and configures the raven store
  /// </summary>
  protected IDocumentStore CreateRavenStore(IServiceProvider services)
  {
    IMixtapeOptions options = services.GetService<IMixtapeOptions>();
    RavenOptions ravenOptions = options.For<RavenOptions>();
    IRavenDocumentConventionsBuilder conventionsBuilder = services.GetService<IRavenDocumentConventionsBuilder>();

    IDocumentStore store = new DocumentStore()
    {
      Database = ravenOptions.Database,
      Urls = [ravenOptions.Url],
      Conventions =
      {
        AggressiveCache =
        {
          Duration = TimeSpan.FromMinutes(ravenOptions.CacheInMinutes),
          Mode = AggressiveCacheMode.TrackChanges
        }
      }
    };

    conventionsBuilder.Run(store.Conventions);

    IDocumentStore raven = store.Initialize();

    // create all indexes
    IEnumerable<IMixtapeIndexDefinition> indexes = ravenOptions.Indexes.BuildAll(options, store); 
    IndexCreation.CreateIndexes(indexes, store, database: ravenOptions.Database);

      
    return raven;
  }
}