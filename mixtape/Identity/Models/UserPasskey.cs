using Microsoft.AspNetCore.Identity;

namespace Mixtape.Identity;

/// <summary>
/// Represents a passkey credential for a user in the identity system.
/// </summary>
/// <remarks>
/// See <see href="https://www.w3.org/TR/webauthn-3/#credential-record"/>.
/// </remarks>
public class UserPasskey
{
  /// <summary>
  /// Id of the user
  /// </summary>
  public string UserId { get; set; }
  
  /// <summary>
  /// Gets or sets the credential ID for this passkey.
  /// </summary>
  public byte[] CredentialId { get; set; }

  /// <summary>
  /// Gets or sets additional data associated with this passkey.
  /// </summary>
  public IdentityPasskeyData Data { get; set; }
}