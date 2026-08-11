using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps.Providers;

public abstract class EntitySitemapProvider<T>(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor) : ISitemapProvider
  where T : ISupportsDateMetadata
{
  /// <summary>
  /// Razor page name which is used to create URL
  /// </summary>
  protected abstract string RouteName { get; }
  
  /// <summary>
  /// Sitemap node priority
  /// </summary>
  protected virtual decimal Priority => 0.5m;
  
  /// <summary>
  /// The usual change frequency (optional)
  /// </summary>
  protected virtual SitemapNodeChangeFrequency? ChangeFrequency => null;
  
  
  /// <summary>
  /// Constructs the sitemap node
  /// </summary>
  protected virtual SitemapNode BuildNode(string url, T model)
  {
    return new SitemapNode(url)
    {
      Priority = Priority,
      ChangeFrequency = ChangeFrequency,
      LastModificationDate = model.LastModifiedDate.DateTime
    };
  }


  /// <summary>
  /// Retrieves all items which should be converted to nodes
  /// </summary>
  protected abstract Task<IReadOnlyCollection<T>> GetItems();


  /// <summary>
  /// Get all routes values which are used for URL creation
  /// </summary>
  protected abstract object GetRouteValues(T model);
  
  
  /// <inheritdoc />
  public virtual async Task<IReadOnlyCollection<SitemapNode>> GetNodes()
  {
    List<SitemapNode> nodes = [];
    IReadOnlyCollection<T> items = await GetItems();

    foreach (T item in items)
    {
      string url = linkGenerator.GetUriByPage(
        httpContext: httpContextAccessor.HttpContext!, 
        page: RouteName,
        values: GetRouteValues(item));

      if (url.HasValue())
      {
        nodes.Add(BuildNode(url, item));
      }
    }

    return nodes;
  }
}