using Microsoft.Extensions.Logging;

namespace Mixtape.Mails.Dispatchers;

/// <summary>
/// Default implementation of an IMailSender which sends the mail to the attached logger
/// and therefore not using the SMTP channel.
/// Implementing real mail sending is up to the consuming application.
/// </summary>
public class LoggerMailDispatcher(ILogger<LoggerMailDispatcher> logger) : IMailDispatcher
{
  /// <inheritdoc />
  public async Task Send(Mail message, CancellationToken token = default)
  {
    logger.LogInformation("Mail to {to}. Subject: {subject}", message.To[0].Address, message.Subject);
  }


  /// <inheritdoc />
  public void Dispose() { }
}