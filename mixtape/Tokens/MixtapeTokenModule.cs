using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mixtape.Tokens;

internal class MixtapeTokenModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IMixtapeTokenProvider, MixtapeTokenProvider>();
    services.AddScoped<IMixtapeTokenStoreDbProvider, EmptyMixtapeTokenStoreDbProvider>();
  }
}