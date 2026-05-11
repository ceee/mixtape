using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using System.Globalization;

namespace Mixtape.Media.ImageSharp.Processors;

public class RotateWebProcessor : IImageWebProcessor
{
  // <summary>
  /// The command constant for quality.
  /// </summary>
  public const string Rotate = "rotate";

  /// <summary>
  /// The reusable collection of commands.
  /// </summary>
  private static readonly IEnumerable<string> RotateCommands
      = [Rotate];

  /// <inheritdoc/>
  public IEnumerable<string> Commands { get; } = RotateCommands;

  /// <inheritdoc/>
  public FormattedImage Process(
      FormattedImage image,
      ILogger logger,
      CommandCollection commands,
      CommandParser parser,
      CultureInfo culture)
  {
    if (commands.Contains(Rotate))
    {
      float degrees = parser.ParseValue<float>(commands.GetValueOrDefault(Rotate), culture);

      if (degrees != 0)
      {
        image.Image.Mutate(x => x.Rotate(degrees));
      }
    }

    return image;
  }

  /// <inheritdoc/>
  public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => true;
}
