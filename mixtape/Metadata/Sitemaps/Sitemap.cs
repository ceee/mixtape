using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Mixtape.Metadata.Sitemaps.Models;
using Mixtape.Metadata.Sitemaps.Providers;

namespace Mixtape.Metadata.Sitemaps;

public class Sitemap(IEnumerable<ISitemapProvider> providers) : ISitemap
{
  /// <inheritdoc />
  public async Task<MemoryStream> GenerateXml()
  {
    // retrieve nodes from all providersj
    List<SitemapNode> nodes = [];
    foreach (ISitemapProvider provider in providers)
    {
      nodes.AddRange(await provider.GetNodes());
    }

    // build root with child nodes
    SitemapRoot model = new(nodes);

    // create serializer with settings
    XmlSerializer serializer = new(typeof(SitemapRoot));
    
    // write xml to memory stream
    MemoryStream stream = new();
    serializer.Serialize(stream, model);
    
    // reset the stream to the start
    stream.Position = 0;
    return stream;
  }
}


public interface ISitemap
{
  /// <summary>
  /// Generate the sitemap retrieving nodes from all registered <see cref="ISitemapProvider"/>.
  /// </summary>
  /// <returns>Open stream which contains the complete XML string</returns>
  Task<MemoryStream> GenerateXml();
}