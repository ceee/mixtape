using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using System.Globalization;

namespace Mixtape.Media.ImageSharp.Processors;

public class BlurWebProcessor : IImageWebProcessor
{
  // <summary>
  /// The command constant for the gaussian blur sigma.
  /// </summary>
  public const string Blur = "blur";

  /// <summary>
  /// The reusable collection of commands.
  /// </summary>
  private static readonly IEnumerable<string> BlurCommands = [Blur];

  /// <inheritdoc/>
  public IEnumerable<string> Commands { get; } = BlurCommands;

  /// <inheritdoc/>
  public FormattedImage Process(
      FormattedImage image,
      ILogger logger,
      CommandCollection commands,
      CommandParser parser,
      CultureInfo culture)
  {
    if (commands.Contains(Blur))
    {
      float sigma = parser.ParseValue<float>(commands.GetValueOrDefault(Blur), culture);

      if (sigma != 0)
      {
        image.Image.Mutate(x => x.GaussianBlur(sigma));
      }
    }

    return image;
  }

  /// <inheritdoc/>
  public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => true;
}
