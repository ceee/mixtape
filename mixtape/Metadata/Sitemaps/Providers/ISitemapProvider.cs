using Mixtape.Metadata.Sitemaps.Models;

namespace Mixtape.Metadata.Sitemaps.Providers;

public interface ISitemapProvider
{
  /// <summary>
  /// Get all nodes for this provider
  /// </summary>
  Task<IReadOnlyCollection<SitemapNode>> GetNodes();
}