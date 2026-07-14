namespace Mixtape.Mails;

public enum MailSuppressionReason
{
  /// <summary>
  /// Not suppressed
  /// </summary>
  None = 0,
  /// <summary>
  /// Suppressed after permanent delivery failure
  /// </summary>
  HardBounce = 1,
  /// <summary>
  /// Suppressed after a recipient reports spam
  /// </summary>
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
