using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps.Providers;

public class PagesSitemapProvider(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider) 
  : ISitemapProvider
{
  /// <inheritdoc/>
  public Task<IReadOnlyCollection<SitemapNode>> GetNodes()
  {
    HttpContext httpContext = httpContextAccessor.HttpContext!;
    List<SitemapNode> nodes = [];

    foreach (ActionDescriptor descriptor in actionDescriptorCollectionProvider.ActionDescriptors.Items)
    {
      // get the closest SitemapAttribute to the endpoint
      object attribute = descriptor.EndpointMetadata.LastOrDefault(em => em is SitemapAttribute);
      
      // skip if the attribute is not found
      if (attribute is not SitemapAttribute sitemapAttribute)
      {
        continue;
      }

      // build URL for razor page or mvc action
      string url = descriptor switch
      {
        PageActionDescriptor razorPage => linkGenerator.GetUriByPage(httpContext, page: razorPage.ViewEnginePath),
        ControllerActionDescriptor controller => linkGenerator.GetUriByAction(httpContext, action: controller.ActionName, controller: controller.ControllerName, values: sitemapAttribute.RouteValues),
        _ => null
      };
      
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
    
    return Task.FromResult<IReadOnlyCollection<SitemapNode>>(nodes);
  }
}