using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Mixtape.Metadata.Sitemaps.Models;

[XmlRoot("url", Namespace =  "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class SitemapNode
{
    internal SitemapNode() { }

    /// <summary>
    /// Creates a sitemap node
    /// </summary>
    /// <param name="url">Specifies the URL</param>
    public SitemapNode(string url)
    {
        Url = url;
    }
    
    /// <summary>
    /// URL of the page
    /// </summary>
    [XmlElement("loc", Order = 1), Url]
    public string Url { get; set; } = null!;


    /// <summary>
    /// Shows the date the URL was last modified, value is optional.
    /// </summary>
    [XmlElement("lastmod", Order = 2)]
    public DateTime? LastModificationDate { get; set; }


    /// <summary>
    /// How frequently the page is likely to change. 
    /// This value provides general information to search engines and may not correlate exactly to how often they crawl the page.
    /// </summary>
    [XmlElement("changefreq", Order = 3)]
    public SitemapNodeChangeFrequency? ChangeFrequency { get; set; }


    /// <summary>
    /// The priority of this URL relative to other URLs on your site. Valid values range from 0.0 to 1.0. This value does not affect how your pages are compared to pages on other sites—it only lets the search engines know which pages you deem most important for the crawlers.
    /// The default priority of a page is 0.5.
    /// Please note that the priority you assign to a page is not likely to influence the position of your URLs in a search engine's result pages.
    /// Search engines may use this information when selecting between URLs on the same site, 
    /// so you can use this tag to increase the likelihood that your most important pages are present in a search index.
    /// Also, please note that assigning a high priority to all of the URLs on your site is not likely to help you.
    /// Since the priority is relative, it is only used to select between URLs on your site.
    /// </summary>
    [XmlElement("priority", Order = 4)]
    public decimal? Priority { get; set; }
}