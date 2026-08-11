namespace Mixtape.Identity;

/// <summary>
/// Represents all the options you can use to configure the cookies middleware used by the identity system.
/// </summary>
public class MixtapeIdentityConstants
{
  public static class CookieNames
  {
    private const string CookiePrefix = "mixtape.id";
    
    public static readonly string Application = CookiePrefix + ".app";
    public static readonly string External = CookiePrefix + ".ext";
    public static readonly string TwoFactorRememberMe = CookiePrefix + ".2fa_rem";
    public static readonly string TwoFactorUserId = CookiePrefix + ".2fa_id";
  }

  public static partial class Claims
  {
    private const string ClaimPrefix = "mixtape.claim";
    
    public static readonly string IsMixtape = ClaimPrefix + ".ismixtape";
    public static readonly string UserId = ClaimPrefix + ".userid";
    public static readonly string Username = ClaimPrefix + ".username";
    public static readonly string Name = ClaimPrefix + ".name";
    public static readonly string Nickname = ClaimPrefix + ".nickname";
    public static readonly string Role = ClaimPrefix + ".rolealias";
    public static readonly string SecurityStamp = ClaimPrefix + ".securitystamp";
    public static readonly string Email = ClaimPrefix + ".email";
    public static readonly string Permission = ClaimPrefix + ".permission";
    public static readonly string TicketExpires = ClaimPrefix + ".ticketExpires";
  }
}
