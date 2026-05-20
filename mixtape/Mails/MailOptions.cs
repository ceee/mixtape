using Mixtape.Mails.Dispatchers.Lettermint;
using Mixtape.Mails.Dispatchers.Postmark;
using Mixtape.Mails.Dispatchers.Scaleway;

namespace Mixtape.Mails;

public class MailOptions
{
  public string From { get; set; }

  public string FromName { get; set; }

  public string To { get; set; }

  public string ToName { get; set; }

  public string ReplyTo { get; set; }

  public bool Debug { get; set; }

  public string SenderEmail { get; set; }

  public string SenderName { get; set; }

  public PostmarkOptions Postmark { get; set; }

  public ScalewayOptions Scaleway { get; set; }

  public LettermintOptions Lettermint { get; set; }

  public Func<Mail, string> BuildViewPath { get; set; }
}