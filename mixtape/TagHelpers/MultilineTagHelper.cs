using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Mixtape.TagHelpers;

[HtmlTargetElement("app-multiline", Attributes = "text")]
public class MultilineTagHelper : TagHelper
{
  [HtmlAttributeName("text")]
  public string Text { get; set; }

  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    output.TagName = string.Empty;
    output.TagMode = TagMode.StartTagAndEndTag;
    output.Content.SetHtmlContent(Text.NewLinesToBr());
  }
}