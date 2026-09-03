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

  public class ListSuppressions
  {
    [JsonPropertyName("data")]
    public ListSuppressionsItem[] Suppressions { get; set; } = [];
    
    [JsonPropertyName("message")]
    public string ErrorMessage { get; set; }
  }
  
  public class ListSuppressionsItem
  {
    public string Id { get; set; }
    
    public SuppressionType Type { get; set; }
    
    public string Value { get; set; }

    public MailSuppressionReason Reason { get; set; } = MailSuppressionReason.None;
    
    public SuppressionScope Scope { get; set; }
    
    public string ProjectId { get; set; }
    
    public string RouteId { get; set; }
    
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedDate { get; set; }
    
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedDate { get; set; }
  }

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum SuppressionType
  {
    Email,
    Domain,
    Extension
  }

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum SuppressionScope
  {
    Global,
    Team, 
    Project,
    Route
  }
}