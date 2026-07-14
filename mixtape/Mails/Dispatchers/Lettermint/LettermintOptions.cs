namespace Mixtape.Mails.Dispatchers.Lettermint;

public class LettermintOptions
{
  public string ApiUrl { get; set; } = "https://api.lettermint.co";

  public string Token { get; set; }
  
  public string TeamToken { get; set; }

  public string Route { get; set; } = "outgoing";
}