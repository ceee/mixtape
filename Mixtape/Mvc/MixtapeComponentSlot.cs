using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Mixtape.Mvc;

[HtmlTargetElement("slot")]
[HtmlTargetElement(Attributes = "slot")]
public class MixtapeComponentSlot(ILogger<MixtapeComponentSlot> logger) : MixtapeComponent
{
  [HtmlAttributeName("name")]
  public string AttributeName { get; set; } = "";

  [HtmlAttributeName("slot")] 
  public string AttributeSlot { get; set; } = "";

  public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
  {
    ArgumentNullException.ThrowIfNull(ParentComponent);
    
    // get slot name
    string name = output.TagName == "slot" ? AttributeName : AttributeSlot;
    ArgumentException.ThrowIfNullOrEmpty(name);

    // get the content of the slot and insert it in the parent component.
    TagHelperContent childContent = await output.GetChildContentAsync();
    
    // remove the slot attribute from the eventual rendered element.
    output.Attributes.RemoveAll("slot");

    ArgumentNullException.ThrowIfNull(childContent);

    bool result = ParentComponent.NamedSlots.TryAdd(name, childContent);

    if (!result)
    {
      logger.LogWarning("A slot identifier has been repeated. Slots require unique name values when used inside a single parent element");
    }

    output.SuppressOutput();
  }
}



// [HtmlTargetElement("slot")]
// public sealed class MixtapeSlotTagHelper : TagHelper
// {
//   [HtmlAttributeName("name")]
//   public string Name { get; set; } = "";
//
//   public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//   {
//     ArgumentException.ThrowIfNullOrEmpty(Name);
//
//     if (!context.Items.TryGetValue(typeof(MixtapeComponentContext), out object value) ||
//         value is not MixtapeComponentContextBuilder builder)
//     {
//       throw new InvalidOperationException("<slot> can only be used inside a Mixtape component.");
//     }
//
//     TagHelperContent content = await output.GetChildContentAsync();
//
//     if (!builder.Slots.TryAdd(Name, content))
//     {
//       throw new InvalidOperationException($"The slot '{Name}' has already been defined. Slot names must be unique within a component.");
//     }
//
//     output.SuppressOutput();
//   }
// }