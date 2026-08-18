using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mixtape.TagHelpers;

[HtmlTargetElement("app-icon", TagStructure = TagStructure.NormalOrSelfClosing)]
public class IconTagHelper : TagHelper
{
  public string Symbol { get; set; }

  public int Size { get; set; } = -1;

  public decimal Stroke { get; set; } = -1;

  public string Class { get; set; }

  public string Set { get; set; }

  [HtmlAttributeNotBound]
  [ViewContext]
  public ViewContext ViewContext { get; set; } = null!;


  private IconOptions _options;

  private readonly IFileVersionProvider _fileVersionProvider;

  private readonly ILogger<IconTagHelper> _logger;


  public IconTagHelper(IOptionsMonitor<IconOptions> options, ILogger<IconTagHelper> logger, IFileVersionProvider fileVersionProvider)
  {
    _options = options.CurrentValue;
    _logger = logger;
    _fileVersionProvider = fileVersionProvider;
    options.OnChange(val => _options = val);
  }


  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    IconSetOptions set = Set.HasValue() ? _options.Sets.FirstOrDefault(x => x.Key == Set) : _options;

    if (set == null || set.Path.IsNullOrWhiteSpace())
    {
      _logger.LogWarning("Could not render <app-icon />. Could not find icon set {set}.", Set);
      output.SuppressOutput();
      return;
    }
    
    int size = Size < 0 ? (set.DefaultSize ?? _options.DefaultSize ?? 18) : Size;
    decimal stroke = Stroke < 0 ? (set.DefaultStrokeWidth ?? _options.DefaultStrokeWidth ?? 2) : Stroke;
    string strokeStr = stroke.ToString().Replace(',', '.');

    string[] classes = new[] { set.CssClass.HasValue() ? set.CssClass : _options.CssClass, Class }.Where(x => x.HasValue()).Distinct().ToArray();

    output.TagName = "svg";
    output.TagMode = TagMode.StartTagAndEndTag;
    output.Attributes.SetAttribute("class", string.Join(" ", classes));
    output.Attributes.SetAttribute("width", size);
    output.Attributes.SetAttribute("height", size);
    output.Attributes.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
    output.Attributes.SetAttribute("stroke-width", strokeStr);
    output.Attributes.SetAttribute("data-symbol", Symbol);
    output.Content.SetHtmlContent(BuildIcon(Symbol, size, strokeStr, setKey: Set));
  }


  public string BuildIcon(string symbol, int size = 18, string stroke = "2", string classes = null, bool withSvg = false, string setKey = null)
  {
    IconSetOptions set = setKey.HasValue() ? _options.Sets.FirstOrDefault(x => x.Key == setKey) : _options;

    if (set == null || set.Path.IsNullOrWhiteSpace())
    {
      _logger.LogWarning("Could not render <app-icon /> (build icon). Could not find icon set {set}.", Set);
      return null;
    }

    string path = _fileVersionProvider.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, set.Path);
    string inner = $"<use xlink:href='{path}#{symbol}\'></use>";

    if (!withSvg)
    {
      return inner;
    }

    return $"<svg class='{(classes.HasValue() ? " " + classes : string.Empty)}' width='{size}' height='{size}'" +
      $"xmlns='http://www.w3.org/2000/svg' stroke-width='{stroke}' data-symbol='{symbol}'>{inner}</svg>";
  }
}