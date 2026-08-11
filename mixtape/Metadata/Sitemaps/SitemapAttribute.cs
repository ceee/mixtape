using Microsoft.AspNetCore.Routing;
using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SitemapAttribute : Attribute
{
  public RouteValueDictionary RouteValues { get; set; }
  
  public DateTime? LastModificationDate { get; set; }
  
  public SitemapNodeChangeFrequency? ChangeFrequency { get; set; }

  public decimal Priority { get; set; } = 0.5m;
}