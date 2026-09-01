using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Mixtape.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Mixtape.Logging;

[ApiController]
[Route("/api/hello")]
public class AnalyticsController : MixtapeController
{
  protected IOptionsMonitor<AnalyticsOptions> Options { get; }
  protected ILogger<AnalyticsController> Logger { get; }
  protected IMemoryCache Cache { get; }
  protected HttpClient Http { get; }

  public AnalyticsController(IOptionsMonitor<AnalyticsOptions> options, ILogger<AnalyticsController> logger, IMemoryCache cache, HttpClient http)
  {
    Options = options;
    Logger = logger;
    Cache = cache;
    Http = http;

    // clear script cache when options change
    options.OnChange(_ =>
    {
      cache.Remove(ScriptCacheKey);
    });
  }


  private const string ScriptCacheKey = "umami_script_cache";

  [HttpGet("friend.js")]
  public async Task<IActionResult> GetTrackerJsFile()
  {
    if (!Options.CurrentValue.ProxyUrl.HasValue())
    {
      Logger.LogWarning("No proxy defined for umami analytics. Cancelling script load");
      return NotFound();
    }

    // try to deliver the file from cache
    if (Cache.TryGetValue(ScriptCacheKey, out string cachedString) && cachedString is not null)
    {
      return Content(cachedString, "text/javascript");
    }

    // load and cache the JS file
    try
    {
      string uri = Options.CurrentValue.ProxyUrl + Options.CurrentValue.ProxyScriptEndpoint;
      string response = await Http.GetStringAsync(uri, HttpContext.RequestAborted);

      // do not push tracking-id via data-attribute but inject it into the file directly
      const string replacementText = "(\"website-id\")";
      int websiteIdIndex = response.IndexOf(replacementText, StringComparison.Ordinal) - 1;
      response = response
        .Remove(websiteIdIndex, replacementText.Length + 1)
        .Insert(websiteIdIndex, $"\"{Options.CurrentValue.TrackingId}\"");

      Cache.Set(ScriptCacheKey, response, new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
      });

      return Content(response, "text/javascript");
    }
    catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
    {
      // ignore
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Failed to load umami script");
      return new StatusCodeResult(500);
    }

    return NotFound();
  }


  [HttpPost("ping")]
  public async Task<IActionResult> Ping()
  {
    if (!Options.CurrentValue.ProxyUrl.HasValue())
    {
      Logger.LogWarning("No proxy defined for umami analytics. Cancelling ping");
      return NotFound();
    }

    // create proxy request
    string uri = Options.CurrentValue.ProxyUrl + Options.CurrentValue.ProxyTrackEndpoint;
    using HttpRequestMessage proxyRequest = new(HttpMethod.Post, uri);

    // forward real IP
    proxyRequest.Headers.TryAddWithoutValidation("X-Forwarded-For", HttpContext.Connection.RemoteIpAddress?.ToString());
    proxyRequest.Headers.TryAddWithoutValidation("X-Real-IP", HttpContext.Connection.RemoteIpAddress?.ToString());

    // append additional headers from client
    foreach (KeyValuePair<string, StringValues> header in Request.Headers)
    {
      if (!proxyRequest.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]))
      {
        proxyRequest.Content ??= new StreamContent(Request.Body);
        proxyRequest.Content.Headers.TryAddWithoutValidation(header.Key, [.. header.Value]);
      }
    }
    proxyRequest.Content ??= new StreamContent(Request.Body);

    // add requested content-type
    if (!string.IsNullOrEmpty(Request.ContentType))
    {
      proxyRequest.Content.Headers.TryAddWithoutValidation("Content-Type", Request.ContentType);
    }

    // send proxy request to umami
    try
    {
      using HttpResponseMessage proxyResponse = await Http.SendAsync(proxyRequest, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);
      string responseBody = await proxyResponse.Content.ReadAsStringAsync(HttpContext.RequestAborted);

      return StatusCode((int)proxyResponse.StatusCode, responseBody);
    }
    catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
    {
      // ignore
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Failed to contact umami service");
      return new StatusCodeResult(500);
    }

    return NotFound();
  }
}