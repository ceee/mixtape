using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

// the following code is heavily inspired from:
// https://github.com/techgems/static-components/blob/master/src/StaticComponents/StaticComponent.cs

namespace Mixtape.Mvc;

public class MixtapeComponent : TagHelper
{
  private const string ComponentStackKey = "StackKey";
  
  /// <summary>
  /// Name of the view
  /// </summary>
  protected virtual string RazorViewName { get; }
  
  /// <summary>
  /// View path
  /// </summary>
  protected virtual string RazorViewPath { get; }

  /// <summary>
  /// The parent component reference. Used internally to track the parent component in the component stack. It is marked with a public get so ASP.NET Core can set its value during runtime, but its value should not be modified manually.
  /// </summary>
  [HtmlAttributeNotBound]
  internal MixtapeComponent ParentComponent { get; set; }

  /// <summary>
  /// The View Context necessary to get the Html Helper that renders the partial views. 
  /// It is marked with a public get so ASP.NET Core can set its value during runtime, but its value should not be modified manually.
  /// </summary>
  [HtmlAttributeNotBound]
  [ViewContext]
  public ViewContext ViewContext { protected get; set; }

  /// <summary>
  /// Child content for rendering in the razor template.
  /// </summary>
  [HtmlAttributeNotBound]
  public TagHelperContent ChildContent { get; set; }

  [HtmlAttributeNotBound]
  internal Dictionary<string, TagHelperContent> NamedSlots { get; set; } = [];

  /// <summary>
  /// Property used for determining if you need a fallback on your child content.
  /// </summary>
  [HtmlAttributeNotBound]
  public bool IsChildContentNullOrEmpty => ChildContent is null || ChildContent.IsEmptyOrWhiteSpace;
  
  /// <summary>
  /// Whether the output is suppressed or not
  /// </summary>
  private bool IsSuppressed { get; set; }


  protected MixtapeComponent()
  {
    RazorViewName = GetViewName(GetType());
    RazorViewPath = GetViewPath(GetType());
  }

  protected MixtapeComponent(string razorViewPath)
  {
    RazorViewName = null;
    RazorViewPath = razorViewPath;
  }
  
  
  /// <summary>
  /// Build view name
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private string GetViewName(Type type)
  {
    string name = type.Name;

    foreach (string suffix in new[] { "TagHelper", "ViewComponent", "Component" })
    {
      if (name.EndsWith(suffix, StringComparison.Ordinal))
      {
        name = name[..^suffix.Length];
        break;
      }
    }

    return name;
  }
  
  
  /// <summary>
  /// Build view path
  /// </summary>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  private string GetViewPath(Type type)
  {
    string assemblyName = type.Assembly.GetName().Name ?? throw new InvalidOperationException();
    string @namespace = type.Namespace ?? "";

    string relativeNamespace = @namespace
      .StartsWith(assemblyName + ".", StringComparison.Ordinal)
      ? @namespace[(assemblyName.Length + 1)..]
      : @namespace;

    return $"~/{relativeNamespace.Replace('.', '/')}/{RazorViewName}.cshtml";
  }

  /// <summary>
  /// Gets the Html Helper from the View Context. Used for rendering partial views.
  /// </summary>
  /// <returns></returns>
  /// <exception cref="ArgumentNullException"></exception>
  protected IHtmlHelper GetHtmlHelper()
  {
    ArgumentNullException.ThrowIfNull(ViewContext);
    IHtmlHelper htmlHelper = ViewContext.HttpContext.RequestServices.GetService<IHtmlHelper>();
    ArgumentNullException.ThrowIfNull(htmlHelper);

    (htmlHelper as IViewContextAware)!.Contextualize(ViewContext);

    return htmlHelper;
  }

  private Stack<MixtapeComponent> GetParentComponentStack(TagHelperContext context)
  {
    return (context.Items[ComponentStackKey] as Stack<MixtapeComponent>)!;
  }

  
  /// <summary>
  /// Render the content of a slot in the base razor view.
  /// </summary>
  /// <param name="name"></param>
  /// <returns></returns>
  public TagHelperContent Slot(string name)
  {
    bool result = NamedSlots.TryGetValue(name, out TagHelperContent slot);

    if (!result)
    {
      return new DefaultTagHelperContent();
    }

    return slot!;
  }
  
  
  /// <summary>
  /// Check if a slot is null/empty
  /// </summary>
  public bool HasSlot(string name)
  {
    return NamedSlots.TryGetValue(name, out TagHelperContent slot) && slot is not null && !slot.IsEmptyOrWhiteSpace;
  }
  

  /// <inheritdoc/>
  public sealed override void Init(TagHelperContext context)
  {
    if (!context.Items.ContainsKey(ComponentStackKey))
    {
      Stack<MixtapeComponent> parentComponentStack = new();

      ParentComponent = null;
      parentComponentStack.Push(this);

      context.Items[ComponentStackKey] = parentComponentStack;
    }
    else
    {
      Stack<MixtapeComponent> parentComponentStack = GetParentComponentStack(context);

      ParentComponent = parentComponentStack.Peek();

      if (this is not MixtapeComponentSlot)
      {
        parentComponentStack.Push(this);
      }
    }

    base.Init(context);
  }

  /// <summary>
  /// Default ProcessAsync method. Will render the default razor view if a route is not provided in the base class.
  /// </summary>
  /// <param name="context"></param>
  /// <param name="output"></param>
  /// <returns></returns>
  /// <exception cref="ArgumentNullException"></exception>
  public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
  {
    ArgumentNullException.ThrowIfNull(ViewContext);
    ArgumentNullException.ThrowIfNull(RazorViewPath);

    await RenderPartialView(RazorViewPath, output);

    if (this is not MixtapeComponentSlot)
    {
      Stack<MixtapeComponent> stack = GetParentComponentStack(context);
      if (stack.Count > 0 && stack.Peek() == this)
      {
        stack.Pop();
      }
    }
  }

  /// <summary>
  /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
  /// Will send the child class as the view model for the partial view.
  /// </summary>
  /// <param name="output"></param>
  /// <returns></returns>
  protected async Task RenderPartialView(TagHelperOutput output)
  {
    await RenderPartialView(RazorViewPath, output, this);
  }

  /// <summary>
  /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
  /// Will send the child class as the view model for the partial view.
  /// </summary>
  /// <param name="viewRoute"></param>
  /// <param name="output"></param>
  /// <returns></returns>
  protected async Task RenderPartialView(string viewRoute, TagHelperOutput output)
  {
    await RenderPartialView(viewRoute, output, this);
  }

  /// <summary>
  /// Uses the HtmlHelper to render the partial view. Defaults tag name to null and adds child content if there was any.
  /// Will send the child class as the view model for the partial view.
  /// </summary>
  /// <param name="viewRoute"></param>
  /// <param name="output"></param>
  /// <param name="model"></param>
  /// <returns></returns>
  protected async Task RenderPartialView<T>(string viewRoute, TagHelperOutput output, T model)
  {
    await InvokeAsync();

    if (IsSuppressed)
    {
      output.TagName = null;
      output.SuppressOutput();
      return;
    }
    
    TagHelperContent childContent = await output.GetChildContentAsync();

    if (childContent is not null)
    {
      ChildContent = childContent;
    }
    
    IHtmlHelper htmlHelper = GetHtmlHelper();
    IHtmlContent content = await htmlHelper.PartialAsync(viewRoute, model);
    output.Content.SetHtmlContent(content);
    output.TagName = null;
  }


  /// <summary>
  /// Invokes before the component is rendered
  /// </summary>
  protected virtual Task InvokeAsync()
  {
    Invoke();
    return Task.CompletedTask;
  }
  

  /// <summary>
  /// Invokes before the component is rendered
  /// </summary>
  protected virtual void Invoke() { }
  
  
  /// <summary>
  /// Suppress output of the component
  /// </summary>
  protected void SuppressOutput()
  {
    IsSuppressed = true;
  }
}