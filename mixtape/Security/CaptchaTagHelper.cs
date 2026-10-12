using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Mixtape.Security;

namespace Mixtape.TagHelpers;

[HtmlTargetElement("app-captcha", TagStructure = TagStructure.NormalOrSelfClosing)]
public class CaptchaTagHelper(IOptionsMonitor<CaptchaOptions> options) : TagHelper
{
  [HtmlAttributeNotBound]
  [ViewContext]
  public ViewContext ViewContext { get; set; } = null!;

  [HtmlAttributeName("for")]
  public ModelExpression For { get; set; }

  [HtmlAttributeName("name")]
  public string Name { get; set; }

  [HtmlAttributeName("lang")]
  public string Lang { get; set; }

  private readonly CaptchaOptions _options = options.CurrentValue;


  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    if (!_options.Enabled)
    {
      output.SuppressOutput();
      return;
    }

    string fieldName = Name.Or("Captcha");

    if (For != null)
    {
      fieldName = NameAndIdProviderCopy.GetFullHtmlFieldName(ViewContext, For.Name);
    }

    output.TagName = "cap-widget";
    output.Attributes.SetAttribute("class", "cap-widget");
    output.Attributes.SetAttribute("data-cap-api-endpoint", _options.Endpoint);
    output.Attributes.SetAttribute("data-cap-hidden-field-name", fieldName);
    output.Attributes.SetAttribute("data-cap-lang", Lang);

    string wasmFilePath =_options.Endpoint + "/cap.wasm";
    string widgetFilePath =_options.Endpoint + "/cap.widget.js";
    output.PreElement.AppendHtml($"<script>window.CAP_CUSTOM_WASM_URL = '{wasmFilePath}';</script>");
    output.PreElement.AppendHtml($"<script type='module' src='{widgetFilePath}'></script>");

    CaptchaLocalizationOptions texts = _options.Localization;

    if (Lang == "en")
    {
      texts = CaptchaLocalizationOptions.English;
    }

    foreach ((string key, string value) in texts)
    {
      output.Attributes.SetAttribute($"data-cap-i18n-{key}", value);
    }
  }
}