namespace Mixtape.Mails.Dispatchers;

public interface IMailDispatcher : IDisposable
{
  /// <summary>
  /// Sends a mail message
  /// </summary>
  Task Send(Mail message, CancellationToken token = default);

  /// <summary>
  /// Whether a certain sender signature is supported by this dispatcher
  /// </summary>
  Task<bool> IsSenderSupported(string email, CancellationToken token = default) => Task.FromResult(true);
  
  /// <summary>
  /// Check whether a certain email address is suppressed
  /// </summary>
  Task<MailSuppressionReason> GetSuppressionReason(string email, CancellationToken token = default) => Task.FromResult(MailSuppressionReason.None);
}