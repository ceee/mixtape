namespace Mixtape.Logging;

public class AnalyticsOptions
{
  public bool Enabled { get; set; } = true;

  public string Endpoint { get; set; } = "/api/hello";

  public string ProxyUrl { get; set; }

  public string ProxyScriptEndpoint { get; set; } = "/hi.js";

  public string ProxyTrackEndpoint { get; set; } = "/ping";

  /// <summary>
  /// Website ID configured in Umami
  /// </summary>
  public string TrackingId { get; set; }
  
  /// <summary>
  /// By default, Umami initializes pageview tracking, click tracking, path change detection, and optional features like performance tracking.
  /// Set this to false only if you want to disable tracker initialization entirely and send data yourself using the tracker functions.
  /// </summary>
  public bool AutoTrack { get; set; } = true;

  /// <summary>
  /// Performance tracks real-user Core Web Vitals collected directly from your visitors' browsers.
  /// Use it to identify slow pages, spot regressions over time, and compare performance across devices and browsers.
  /// </summary>
  public bool TrackPerformance { get; set; } = true;
  
  /// <summary>
  /// If the app uses hash routing (/#/page), Umami doesn't track these by default.
  /// You can enable hash collection with this option.
  /// </summary>
  public bool TrackHashtags { get; set; } = false;

  public bool Valid()
  {
    return Enabled && TrackingId.HasValue() && ProxyUrl.HasValue();
  }
}