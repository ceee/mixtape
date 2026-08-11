using Microsoft.AspNetCore.Identity;

namespace Mixtape.Identity;

public static class UserExtensions
{
  public static UserPasskeyInfo ToUserPasskeyInfo(this UserPasskey passkey)
  {
    return new UserPasskeyInfo(
        passkey.CredentialId,
        passkey.Data.PublicKey,
        passkey.Data.CreatedAt,
        passkey.Data.SignCount,
        passkey.Data.Transports,
        passkey.Data.IsUserVerified,
        passkey.Data.IsBackupEligible,
        passkey.Data.IsBackedUp,
        passkey.Data.AttestationObject,
        passkey.Data.ClientDataJson)
      {
        Name = passkey.Data.Name
      };
  }
}