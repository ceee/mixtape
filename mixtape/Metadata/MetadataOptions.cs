namespace Mixtape.Metadata;

public class MetadataOptions
{
  public MetadataOptions() { }

  public MetadataOptions(params string[] titleFragments)
  {
    TitleFragments.AddRange(titleFragments);
  }
  
  public List<string> TitleFragments { get; set; } = [];

  public bool? NoIndex { get; set; }

  public bool? NoFollow { get; set; }

  public string Description { get; set; }

  public string Icon { get; set; }

  public string Image { get; set; }

  public string Author { get; set; }

  public string TitleFragmentsSeparator { get; set; } = " / ";

  public string TitlePageNameToFragmentSeparator { get; set; } = " / ";

  public bool HidePageName { get; set; }

  /// <summary>
  /// Additional properties which can be used for templating
  /// </summary>
  public Dictionary<string, string> Properties { get; set; } = [];
}
