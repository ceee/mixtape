namespace Mixtape.Models;

public interface ISupportsDateMetadata
{
  /// <summary>
  /// Date of last modification
  /// </summary>
  DateTimeOffset LastModifiedDate { get; set; }

  /// <summary>
  /// Date of creation
  /// </summary>
  DateTimeOffset CreatedDate { get; set; }
}