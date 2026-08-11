using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps.Providers;

public class EndpointsSitemapProvider(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IApiDescriptionGroupCollectionProvider  apiDescriptionGroupCollectionProvider) 
  : ISitemapProvider
{
  /// <inheritdoc/>
  public Task<IReadOnlyCollection<SitemapNode>> GetNodes()
  {
    HttpContext httpContext = httpContextAccessor.HttpContext!;
    List<SitemapNode> nodes = [];

    foreach (ApiDescriptionGroup descriptor in apiDescriptionGroupCollectionProvider.ApiDescriptionGroups.Items)
    {
      IEnumerable<ApiDescription> endpoints = descriptor.Items
          .Where(i => HttpMethods.IsGet(i.HttpMethod ?? ""))
          .Where(i => i.ActionDescriptor.EndpointMetadata.Any(em => em is SitemapAttribute));

      foreach (ApiDescription endpoint in endpoints)
      {
        // get the closest SitemapAttribute to the endpoint
        object attribute = endpoint.ActionDescriptor.EndpointMetadata.LastOrDefault(a => a is SitemapAttribute);
        
        // skip if the attribute is not found
        if (attribute is not SitemapAttribute sitemapAttribute)
        {
          continue;
        }
        
        // get route names for endpoint
        string routeName = endpoint.ActionDescriptor.EndpointMetadata
          .OfType<RouteNameMetadata>()
          .Select(a => a.RouteName)
          .FirstOrDefault();

        if (routeName is null)
        {
          continue;
        }

        // build URL for endpoint
        string url = linkGenerator.GetUriByName(httpContext, routeName, values: sitemapAttribute.RouteValues);

        // add only valid URLs
        if (url.HasValue() && !nodes.Exists(n => n.Url.Equals(url, StringComparison.OrdinalIgnoreCase)))
        {
          nodes.Add(new SitemapNode(url)
          {
            ChangeFrequency = sitemapAttribute.ChangeFrequency,
            LastModificationDate = sitemapAttribute.LastModificationDate,
            Priority = sitemapAttribute.Priority
          });
        }
      }
    }
    
    return Task.FromResult<IReadOnlyCollection<SitemapNode>>(nodes);
  }
}