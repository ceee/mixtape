using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps;

public static class RouteBuilderExtensions
{
  public static void MapSitemap(this IEndpointRouteBuilder routes, string path)
  {
    routes.MapGet(path, async (ISitemap sitemap) =>
    {
      SitemapResult model = await sitemap.Generate();

      if (!model.HasEntries)
      {
        return Results.NotFound();
      }
      
      return Results.Stream(model.XmlStream, "text/xml");
      
    }).CacheOutput("mixtape.sitemap");
  }
}