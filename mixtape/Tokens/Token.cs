namespace Mixtape.Tokens;

public class SecurityToken : MixtapeIdEntity, ISupportsDbConventions
{
  public string Key { get; set; }

  public string Token { get; set; }

  public Dictionary<string, string> Metadata { get; set; } = new();
  
  public DateTimeOffset Expires { get; set; }
}