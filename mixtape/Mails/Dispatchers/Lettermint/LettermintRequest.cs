using System.Text.Json.Serialization;

namespace Mixtape.Mails.Dispatchers.Lettermint;

public class LettermintRequest
{
  public class SendEmail
  {
    public string From { get; set; }

    public string[] To { get; set; } = [];

    public string[] Cc { get; set; } = [];

    public string[] Bcc { get; set; } = [];

    public string[] ReplyTo { get; set; }

    public string Subject { get; set; }

    public string Text { get; set; }

    public string Html { get; set; }

    public string Route { get; set; }

    public string Tag { get; set; }

    public List<EmailAttachment> Attachments { get; set; } = [];

    public Dictionary<string, string> Metadata { get; set; } = new();
  }

  public class EmailAttachment
  {
    /// <summary>
    /// Filename of the attachment.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// MIME type of the attachment.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Content of the attachment encoded in base64.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Content ID for inline attachments, referenced via cid: in HTML body
    /// </summary>
    public string ContentId { get; set; }
  }
}