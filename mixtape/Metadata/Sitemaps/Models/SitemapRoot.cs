using System.Xml.Serialization;

namespace Mixtape.Metadata.Sitemaps.Models;

/// <summary>
/// Encapsulates the sitemap file and references the current protocol standard.
/// </summary>
[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class SitemapRoot
{
  internal SitemapRoot() 
  { 
    Nodes = [];
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SitemapRoot"/> class.
  /// </summary>
  /// <param name="nodes">Sitemap nodes.</param>
  public SitemapRoot(List<SitemapNode> nodes)
  {
    Nodes = nodes;
  }

  /// <summary>
  /// Sitemap nodes linking to documents
  /// </summary>
  [XmlElement("url")]
  public List<SitemapNode> Nodes { get; }
}