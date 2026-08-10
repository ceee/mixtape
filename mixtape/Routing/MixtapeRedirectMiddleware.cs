using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Mixtape.Routing;

public class MixtapeRedirectMiddleware(RequestDelegate next)
{
  public async Task Invoke(HttpContext httpContext, IOptions<RoutingOptions> options)
  {
    PathString path = httpContext.Request.Path;
    Dictionary<string, string> redirects = options.Value.Redirects;

    // permanently redirect all defined values
    if (redirects.TryGetValue(path, out string destination))
    {
      httpContext.Response.Redirect(destination, permanent: true);
      return;
    }
    
    // remove trailing slash
    if (options.Value.RemoveTrailingSlash && path.HasValue && path.Value.Length > 1 && path.Value[^1] == '/')
    {
      string newPath = path.Value[..^1] + httpContext.Request.QueryString;

      httpContext.Response.Redirect(newPath, permanent: true);
      return;
    }
    
    await next(httpContext);
  }
}