using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mixtape;

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseMixtape(this IApplicationBuilder app)
  {
    IHostEnvironment env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
    
    app.UseMiddleware<MixtapeContextMiddleware>();

    MixtapeBuilder.Modules.Configure(app, app as IEndpointRouteBuilder, app.ApplicationServices);
    
    if (!env.IsDevelopment())
    {
      app.UseResponseCaching();
      app.UseOutputCache();
    }

    if (app is WebApplication webApplication)
    {
      webApplication.MapRazorPages();
      webApplication.MapControllers();
    }

    return app;
  }
}
