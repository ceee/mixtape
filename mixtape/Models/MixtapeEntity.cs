using System.Diagnostics;

namespace Mixtape.Models;

[DebuggerDisplay("Id = {Id,nq}, Name = {Name}, Alias = {Alias}")]
public class MixtapeEntity : MixtapeIdEntity, ISupportsDbConventions, ISupportsRouting, ISupportsFlavors, ISupportsSorting, ISupportsDateMetadata
{
  /// <summary>
  /// Full name of the entity
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Alias (non-unique) which can be used in the frontend and URLs
  /// </summary>
  public string Alias { get; set; }

  /// <summary>
  /// A key which can be used to query this entity in code
  /// </summary>
  public string Key { get; set; }

  /// <summary>
  /// Sort order
  /// </summary>
  public uint Sort { get; set; }

  /// <summary>
  /// Whether the entity is visible in the frontend
  /// </summary>
  public bool IsActive { get; set; } = true;

  /// <summary>
  /// Unique hash for this entity (primarily used for routing)
  /// </summary>
  public string Hash { get; set; }

  /// <summary>
  /// Backoffice user who last modified this content
  /// </summary>
  public string LastModifiedById { get; set; }

  /// <summary>
  /// Date of last modification
  /// </summary>
  public DateTimeOffset LastModifiedDate { get; set; }

  /// <summary>
  /// Backoffice user who created this content
  /// </summary>
  public string CreatedById { get; set; }

  /// <summary>
  /// Date of creation
  /// </summary>
  public DateTimeOffset CreatedDate { get; set; }

  /// <summary>
  /// Alias of the used flavor (uses default if empty)
  /// </summary>
  public string Flavor { get; set; }

  /// <summary>
  /// Additional properties for this entity
  /// </summary>
  public Dictionary<string, string> Properties { get; set; } = new();
}
 