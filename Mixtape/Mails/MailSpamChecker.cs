using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Mixtape.Mails;

public class MailSpamChecker(ILogger<MailSpamChecker> logger) : IMailSpamChecker
{
  private static readonly Regex HasWordLikeContent = new(@"\p{L}{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
  private static readonly Regex AlphanumericToken = new(@"\A[A-Za-z0-9]{10,30}\z", RegexOptions.Compiled | RegexOptions.CultureInvariant);


  /// <inheritdoc />
  public virtual Task<bool> IsLikelySpam(string message, string email)
  {
    if (message.IsNullOrWhiteSpace())
    {
      logger.LogInformation("Flagged mail as spam. Reason: Message is empty");
      return Task.FromResult(true);
    }

    string text = message.Trim();

    // catch long, uninterrupted alphanumeric tokens
    if (AlphanumericToken.IsMatch(text))
    {
      logger.LogInformation("Flagged mail as spam. Reason: Message is an alphanumeric token");
      return Task.FromResult(true);
    }

    // catch messages without any word-like sequence
    bool hasWordLikeContent = HasWordLikeContent.IsMatch(text);

    if (!hasWordLikeContent)
    {
      logger.LogInformation("Flagged mail as spam. Reason: Message does not contain meaningful words (message: {message})", message);
      return Task.FromResult(true);
    }

    // no spam
    return Task.FromResult(false);
  }
}


public interface IMailSpamChecker
{
  /// <summary>
  /// Checks whether a message is probably spam
  /// </summary>
  /// <param name="message"></param>
  /// <param name="email"></param>
  /// <returns></returns>
  Task<bool> IsLikelySpam(string message, string email);
}