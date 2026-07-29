using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mixtape;

public static class ServiceCollectionExtensions
{
  public static MixtapeBuilder AddMixtape(this IServiceCollection services, IConfiguration configuration)
  {
    return new MixtapeBuilder(services, configuration);
  }
}