using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mixtape.Routing;

internal class MixtapeRoutingModule : MixtapeModule
{
  public override int ConfigureOrder { get; } = -10;

  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IRequestUrlResolver, RequestUrlResolver>();
    services.AddOptions<RoutingOptions>().Bind(configuration.GetSection("Mixtape:Routing"));
  }

  public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
  {
    app.UseMiddleware<MixtapeRedirectMiddleware>();
  }
}