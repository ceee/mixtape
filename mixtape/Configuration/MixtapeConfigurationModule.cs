using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mixtape.Configuration;

internal class MixtapeConfigurationModule : MixtapeModule
{
  public override int Order => -1000;

  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddOptions<MixtapeOptions>().Configure<IServiceProvider>((opts, svc) =>
    {
      opts.ServiceProvider = svc;
      opts.Version = "1.0.0-alpha.1";
      opts.TokenExpiration = TimeSpan.FromHours(3);
      opts.DataProtectionStoragePath = "../cache/dpkeys";
    }).Bind(configuration.GetSection("Mixtape"));

    services.AddSingleton<IMixtapeOptions, MixtapeOptions>(factory => factory.GetService<IOptions<MixtapeOptions>>().Value);
  }
}