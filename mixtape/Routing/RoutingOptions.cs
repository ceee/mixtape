namespace Mixtape.Routing;

public class RoutingOptions
{
  public bool RemoveTrailingSlash { get; set; }
  
  public Dictionary<string, string> Redirects { get; set; } = [];
}