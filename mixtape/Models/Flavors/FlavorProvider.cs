using System.Text.Json.Serialization;

namespace Mixtape.Models;

/// <summary>
/// A flavor provider is attached to an entity (which has ISupportsFlavors) and contains all flavors
/// </summary>
public class FlavorProvider
{
  public bool CanUseWithoutFlavors { get; set; } = true;

  public string DefaultFlavor { get; set; }

  [JsonIgnore]
  public Type BaseType { get; set; }

  [JsonIgnore]
  public Type FlavorlessType { get; set; }

  [JsonIgnore]
  public Func<object> FlavorlessConstruct { get; set; }

  public List<FlavorConfig> Flavors { get; set; } = [];

  /// <summary>
  /// Flavor discriminator converter 
  /// </summary>
  [JsonIgnore]
  public Func<FlavorProvider, JsonConverter> ConverterCreator { get; set; }
}