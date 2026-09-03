using System.Text.Json.Serialization;

namespace Mixtape.Mails;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MailSuppressionReason
{
  /// <summary>
  /// Not suppressed
  /// </summary>
  None = 0,
  /// <summary>
  /// Suppressed after permanent delivery failure
  /// </summary>
  [JsonStringEnumMemberName("hard_bounce")]
  HardBounce = 1,
  /// <summary>
  /// Suppressed after a recipient reports spam
  /// </summary>
  [JsonStringEnumMemberName("spam_complaint")]
  SpamComplaint = 2,
  /// <summary>
  /// Manually suppressed via dashboard or API
  /// </summary>
  Manual = 3,
  /// <summary>
  /// Hosted unsubscribe and unsubscribe workflows
  /// </summary>
  Unsubscribe = 4,
  /// <summary>
  /// Unknown reason
  /// </summary>
  Other = 9
}
