using Mixtape.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Mixtape.TagHelpers;

[HtmlTargetElement("app-tracker", TagStructure = TagStructure.NormalOrSelfClosing)]
public class AnalyticsTrackerTagHelper(IOptionsMonitor<AnalyticsOptions> options, IHostEnvironment env) : TagHelper
{
  [HtmlAttributeNotBound]
  [ViewContext]
  public ViewContext ViewContext { get; set; } = null!;

  private readonly AnalyticsOptions _options = options.CurrentValue;

  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    if (!_options.Valid() || !env.IsProduction())
    {
      output.SuppressOutput();
      return;
    }

    output.TagName = "script";
    output.TagMode = TagMode.StartTagAndEndTag;
    output.Attributes.Add(new("async"));
    output.Attributes.SetAttribute("src", _options.Endpoint + "/friend.js");
    // the website is injected into the script
    //output.Attributes.SetAttribute("data-website-id", _options.TrackingId);
    if (_options.TrackPerformance)
    {
      output.Attributes.SetAttribute("data-performance", "true");
    }
    if (_options.TrackHashtags)
    {
      output.Attributes.SetAttribute("data-exclude-hash", "true");
    }
    if (!_options.AutoTrack)
    {
      output.Attributes.SetAttribute("data-auto-track", "false");
    }
  }
}