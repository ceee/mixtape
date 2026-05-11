namespace Mixtape.Identity;

public abstract class MixtapeIdentityRole : MixtapeEntity
{
  /// <summary>
  /// The role's claims, for use in claims-based authentication.
  /// </summary>
  public List<UserClaim> Claims { get; set; } = [];
}
