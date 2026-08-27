using System.Globalization;

namespace Mixtape.Localization;

public class CultureChangeMessage : IMessage
{
  public CultureInfo Culture { get; set; }
  
  public string LanguageCode { get; set; }
}