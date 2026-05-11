using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mixtape.Routing;

public class RequestUrlResolver : IRequestUrlResolver
{
  protected IHttpContextAccessor HttpContextAccessor { get; }

  protected ILogger<RequestUrlResolver> Logger { get; }

  static string[] Protocols = ["http://", "https://", "ftp://", "ftps://", "sftp://", "udp://"];


  public RequestUrlResolver(IHttpContextAccessor httpContextAccessor, ILogger<RequestUrlResolver> logger)
  {
    HttpContextAccessor = httpContextAccessor;
    Logger = logger;
  }


  /// <inheritdoc />
  public bool IsAbsolute(string path)
  {
    if (!CanResolve())
    {
      return false;
    }

    if (string.IsNullOrWhiteSpace(path))
    {
      return false;
    }

    if (Protocols.Any(p => path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
    {
      return true;
    }

    return false;
  }


  /// <inheritdoc />
  public string ToAbsolute(string path)
  {
    if (!CanResolve())
    {
      return null;
    }

    if (string.IsNullOrWhiteSpace(path))
    {
      return GetRoot();
    }

    if (Protocols.Any(p => path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
    {
      return path;
    }

    path = path.Trim().TrimEnd('/');

    if (path.StartsWith("~/"))
    {
      path = path.Substring(1);
    }
    if (!path.StartsWith("/"))
    {
      return GetRoot(includePath: true) + '/' + path;
    }

    return GetRoot() + path;
  }


  /// <inheritdoc />
  public string GetRoot(bool includePath = false)
  {
    if (!CanResolve())
    {
      Logger.LogWarning("Tried to resolve an URL but no HttpRequest is available");
      return null;
    }

    HttpRequest request = HttpContextAccessor.HttpContext.Request;
    return $"{request.Scheme}://{request.Host.Host}{GetPortUrlPart(request)}{(includePath ? request.PathBase.ToUriComponent() : string.Empty)}";
  }


  /// <summary>
  /// Require a request in order to resolve the URL
  /// </summary>
  protected bool CanResolve() => HttpContextAccessor?.HttpContext?.Request != null;


  /// <summary>
  /// Resolves the port as part of the URL
  /// </summary>
  protected string GetPortUrlPart(HttpRequest request)
  {
    if (!request.Host.Port.HasValue || request.Host.Port == 80 || (request.Host.Port == 443 && request.IsHttps))
    {
      return string.Empty;
    }

    return ":" + request.Host.Port;
  }
}


public interface IRequestUrlResolver
{
  /// <summary>
  /// Checks whether a path is absolute or relative
  /// </summary>
  bool IsAbsolute(string path);

  /// <summary>
  /// Converts a relative to an absolute path for the currently used domain
  /// </summary>
  string ToAbsolute(string path);

  /// <summary>
  /// Get protocol + domain and optional port
  /// </summary>
  string GetRoot(bool includePath = false);
}
