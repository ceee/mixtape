using System.Text.Json.Serialization;

namespace Mixtape.Mails.Dispatchers.Lettermint;

public class LettermintResponse
{
  public class SendEmail
  {
    public string MessageId { get; set; }

    public string Status { get; set; }

    [JsonPropertyName("message")]
    public string ErrorMessage { get; set; }
  }
}