using System.IO;

namespace Mixtape.Metadata.Sitemaps.Models;

public class SitemapResult
{
  public MemoryStream XmlStream { get; set; }

  public bool HasEntries => NodeCount > 0;
  
  public int NodeCount { get; set; }
}