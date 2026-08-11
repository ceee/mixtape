using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mixtape.Metadata.Sitemaps;
using Mixtape.Metadata.Sitemaps.Providers;

namespace Mixtape.Metadata;

public class MixtapeMetadataModule : MixtapeModule
{
  public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IMetadataService, MetadataService>();

    services.AddScoped<ISitemapProvider, PagesSitemapProvider>();
    services.AddScoped<ISitemapProvider, EndpointsSitemapProvider>();
    services.AddScoped<ISitemap, Sitemap>();
    
    services.Configure<OutputCacheOptions>(options => 
    {
      options.AddPolicy("mixtape.sitemap", b => b.Expire(TimeSpan.FromDays(1)));
    });
  }


  public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
  {
    routes.MapSitemap("/sitemap.xml");
    
    base.Configure(app, routes, serviceProvider);
  }
}