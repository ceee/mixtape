using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Mixtape.Identity;


public partial class MixtapeUserStore<TUser> :
  IUserStore<TUser>,
  IUserEmailStore<TUser>,
  IUserLockoutStore<TUser>,
  IUserPasswordStore<TUser>,
  IUserClaimStore<TUser>,
  IUserSecurityStampStore<TUser>,
  IUserLoginStore<TUser>,
  IUserAuthenticationTokenStore<TUser>,
  IUserAuthenticatorKeyStore<TUser>,
  IUserTwoFactorStore<TUser>,
  IUserTwoFactorRecoveryCodeStore<TUser>,
  IUserPhoneNumberStore<TUser>,
  IUserPasskeyStore<TUser> 
  where TUser : MixtapeIdentityUser, new()
{
  public MixtapeUserStore(IMixtapeIdentityStoreDbProvider db, IdentityErrorDescriber describer = null)
  {
    Db = db;
    ErrorDescriber = describer ?? new IdentityErrorDescriber();
  }
  
  private StringComparison _comparer = StringComparison.InvariantCultureIgnoreCase;

  protected IdentityErrorDescriber ErrorDescriber { get; private set; }
  
  protected virtual IMixtapeIdentityStoreDbProvider Db { get; set; }
  
  private IdentityResult Fail(Result<TUser> result)
  {
    IdentityError[] errors = new IdentityError[result.Errors.Count];

    int index = 0;
    foreach (ResultError error in result.Errors)
    {
      string message = error.Message + "(key: " + error.Property + ")";
      errors[index++] = new() { Code = "mixtape/raven/500", Description = message };
    }

    return IdentityResult.Failed(errors);
  }

  /// <summary>
  /// Whether an email is already reserved
  /// </summary>
  protected virtual async Task<bool> IsEmailReserved(TUser user, CancellationToken cancellationToken = default)
  {
    // TODO index
    TUser existingUser = await Db.Find<TUser>(x => x.Email == user.Email && !x.IsDeleted, cancellationToken);
    return existingUser != null && existingUser.Id != user.Id;
  }


  /// <inheritdoc />
  public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
  {
    if (await IsEmailReserved(user, cancellationToken))
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "DuplicateEmail",
        Description = $"The email address {user.Email} is already taken."
      });
    }

    Result<TUser> result = await Db.Create(user);
    
    if (!result.IsSuccess)
    {
      return Fail(result);
    }
    
    return IdentityResult.Success;
  }


  /// <inheritdoc />
  public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
  {
    Result<TUser> result = await Db.Delete(user);
      
    if (!result.IsSuccess)
    {
      return Fail(result);
    }
    return IdentityResult.Success;
  }


  /// <inheritdoc />
  public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
  {
    TUser source = await Db.Load<TUser>(user.Id);

    if (source == null)
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "UserNotFound",
        Description = $"Could not find stored user with id {user.Id}"
      });
    }

    if (source.Email != user.Email && await IsEmailReserved(user, cancellationToken))
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "DuplicateEmail",
        Description = $"The email address {user.Email} is already taken."
      });
    }

    Result<TUser> result = await Db.Update(user);
    
    if (!result.IsSuccess)
    {
      return Fail(result);
    }

    return IdentityResult.Success;
  }


  /// <inheritdoc />
  public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
  {
    // TODO index
    return await Db.Load<TUser>(userId);
  }


  /// <inheritdoc />
  public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
  {
    // TODO index
    return await Db.Find<TUser>(x => x.Username == normalizedUserName && !x.IsDeleted, cancellationToken);
  }


  /// <inheritdoc />
  public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Username);


  /// <inheritdoc />
  public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);


  /// <inheritdoc />
  public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Username);


  /// <inheritdoc />
  public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
  {
    user.Username = normalizedName;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken) =>
    SetNormalizedUserNameAsync(user, userName, cancellationToken);


  /// <inheritdoc />
  void IDisposable.Dispose() { }


  /*
    * ****************************************************
    * SECURITY STAMP
    * ****************************************************
    */


  /// <inheritdoc />
  public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken) =>
    Task.FromResult(user.SecurityStamp);


  /// <inheritdoc />
  public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
  {
    user.SecurityStamp = stamp;
    return Task.CompletedTask;
  }


  /*
    * ****************************************************
    * EMAIL
    * ****************************************************
    */


  /// <inheritdoc />
  public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
  {
    // TODO index
    return await Db.Find<TUser>(x => x.Email == normalizedEmail && !x.IsDeleted, cancellationToken);
  }


  /// <inheritdoc />
  public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);


  /// <inheritdoc />
  public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.IsEmailConfirmed);


  /// <inheritdoc />
  public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.Email);


  /// <inheritdoc />
  public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
  {
    user.Email = email.ToLowerInvariant();
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
  {
    user.IsEmailConfirmed = confirmed;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
  {
    user.Email = normalizedEmail.ToLowerInvariant();
    return Task.CompletedTask;
  }


  /*
    * ****************************************************
    * LOCKOUT
    * ****************************************************
    */


  /// <inheritdoc />
  public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.AccessFailedCount);


  /// <inheritdoc />
  public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnabled);


  /// <inheritdoc />
  public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.LockoutEnd);


  /// <inheritdoc />
  public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
  {
    user.AccessFailedCount += 1;
    return Task.FromResult(user.AccessFailedCount);
  }


  /// <inheritdoc />
  public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
  {
    user.AccessFailedCount = 0;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
  {
    user.LockoutEnabled = enabled;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
  {
    user.LockoutEnd = lockoutEnd;
    return Task.CompletedTask;
  }


  /*
    * ****************************************************
    * PASSWORD
    * ****************************************************
    */


  /// <inheritdoc />
  public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);


  /// <inheritdoc />
  public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(!user.PasswordHash.IsNullOrEmpty());


  /// <inheritdoc />
  public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
  {
    user.PasswordHash = passwordHash;
    return Task.CompletedTask;
  }


  /*
    * ****************************************************
    * CLAIM
    * ****************************************************
    */


  /// <inheritdoc />
  public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
  {
    user.Claims.AddRange(claims.Select(claim => new UserClaim(claim)));
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
  {
    return Task.FromResult((IList<Claim>)user.Claims.Select(claim => claim.ToClaim()).ToList());
  }


  /// <inheritdoc />
  public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
  {
    UserClaim userClaim = new(claim);
    // TODO index
    return await Db.FindAll<TUser>(x => !x.IsDeleted && x.Claims.Any(c => c.Type == userClaim.Type && c.Value == userClaim.Value), cancellationToken);
  }


  /// <inheritdoc />
  public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
  {
    IEnumerable<UserClaim> userClaims = claims.Select(c => new UserClaim(c)).ToList();

    user.Claims = user.Claims.Except(userClaims, new UserClaimComparer()).ToList();
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
  {
    UserClaim userClaim = new(claim);
    UserClaim newUserClaim = new(newClaim);

    user.Claims.Remove(userClaim);
    user.Claims.Add(newUserClaim);
    
    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
  {
    user.ExternalLogins.Add(new UserExternalLogin()
    {
      LoginProvider = login.LoginProvider,
      ProviderKey = login.ProviderKey,
      ProviderDisplayName = login.ProviderDisplayName
    });

    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
  {
    UserExternalLogin login = user.ExternalLogins.FirstOrDefault(x =>
      x.LoginProvider.Equals(loginProvider, _comparer) && x.ProviderKey.Equals(providerKey, _comparer));

    if (login != null)
    {
      user.ExternalLogins.Remove(login);
    }
    
    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
  {
    return Task.FromResult<IList<UserLoginInfo>>(user.ExternalLogins.Select(x => x.ToLoginInfo()).ToList());
  }

  
  /// <inheritdoc />
  public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
  {
    // TODO index
    return await Db.Find<TUser>(x => !x.IsDeleted && x.ExternalLogins.Any(l => l.LoginProvider.Equals(loginProvider, _comparer) && l.ProviderKey.Equals(providerKey, _comparer)), cancellationToken);
  }

  
  /// <inheritdoc />
  public Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
  {
    UserToken token = user.Tokens.FirstOrDefault(x => x.LoginProvider.Equals(loginProvider, _comparer) && x.Name.Equals(name, _comparer));

    if (token == null)
    {
      token = new() { LoginProvider = loginProvider, Name = name };
      user.Tokens.Add(token);
    }

    token.Value = value;

    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
  {
    UserToken token = user.Tokens.FirstOrDefault(x =>
      x.LoginProvider.Equals(loginProvider, _comparer) && x.Name.Equals(name, _comparer));

    if (token != null)
    {
      user.Tokens.Remove(token);
    }
    
    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.Tokens.FirstOrDefault(x =>
      x.LoginProvider.Equals(loginProvider, _comparer) && x.Name.Equals(name, _comparer))?.Value);
  }


  /// <inheritdoc />
  public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
  {
    user.TwoFactorAuthenticatorKey = key;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken) =>
    Task.FromResult(user.TwoFactorAuthenticatorKey);


  /// <inheritdoc />
  public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
  {
    user.TwoFactorEnabled = enabled;
    user.TwoFactorEnabledDate = enabled ? DateTimeOffset.Now : null;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken) =>
    Task.FromResult(user.TwoFactorEnabled);


  /// <inheritdoc />
  public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
  {
    user.TwoFactorRecoveryCodes = recoveryCodes;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (user == null)
    {
      throw new ArgumentNullException(nameof(user));
    }
    if (code == null)
    {
      throw new ArgumentNullException(nameof(code));
    }
    

    if (user.TwoFactorRecoveryCodes.Contains(code))
    {
      List<string> updatedCodes = new(user.TwoFactorRecoveryCodes.Where(s => s != code));
      await ReplaceCodesAsync(user, updatedCodes, cancellationToken).ConfigureAwait(false);
      return true;
    }
    return false;
  }

  
  /// <inheritdoc />
  public Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    if (user == null)
    {
      throw new ArgumentNullException(nameof(user));
    }

    return Task.FromResult<int>(user.TwoFactorRecoveryCodes?.Count() ?? 0);
  }


  /// <inheritdoc />
  public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
  {
    user.PhoneNumber = phoneNumber;
    return Task.CompletedTask;
  }


  /// <inheritdoc />
  public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken) =>
    Task.FromResult(user.PhoneNumber);


  /// <inheritdoc />
  public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken) =>
    Task.FromResult(user.IsPhoneNumberConfirmed);


  /// <inheritdoc />
  public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
  {
    user.IsPhoneNumberConfirmed = confirmed;
    return Task.CompletedTask;
  }

  
  /// <inheritdoc />
  public async Task AddOrUpdatePasskeyAsync(TUser user, UserPasskeyInfo passkey, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    
    UserPasskey userPasskey = await FindUserPasskeyByIdAsync(passkey.CredentialId, cancellationToken).ConfigureAwait(false);
    
    if (userPasskey != null)
    {
      // We only mutate properties that can be updated after passkey creation.
      // See https://www.w3.org/TR/webauthn-3/#authn-ceremony-update-credential-record
      userPasskey.Data.Name = passkey.Name;
      userPasskey.Data.SignCount = passkey.SignCount;
      userPasskey.Data.IsBackedUp = passkey.IsBackedUp;
      userPasskey.Data.IsUserVerified = passkey.IsUserVerified;
    }
    else
    {
      userPasskey = CreateUserPasskey(user, passkey);
      user.Passkeys.Add(userPasskey);
    }

    await Db.Update(user, cancellationToken);
  }

  
  /// <inheritdoc />
  public Task<IList<UserPasskeyInfo>> GetPasskeysAsync(TUser user, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    return Task.FromResult<IList<UserPasskeyInfo>>(user.Passkeys.Select(x => x.ToUserPasskeyInfo()).ToList());
  }

  
  /// <inheritdoc />
  public async Task<TUser> FindByPasskeyIdAsync(byte[] credentialId, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    UserPasskey passkey = await FindUserPasskeyByIdAsync(credentialId, cancellationToken).ConfigureAwait(false);
    if (passkey != null)
    {
      return await FindByIdAsync(passkey.UserId, cancellationToken).ConfigureAwait(false);
    }
    return null;
  }

  
  /// <inheritdoc />
  public async Task<UserPasskeyInfo> FindPasskeyAsync(TUser user, byte[] credentialId, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    UserPasskey passkey = await FindUserPasskeyAsync(user.Id, credentialId, cancellationToken).ConfigureAwait(false);
    return passkey?.ToUserPasskeyInfo();
  }

  
  /// <inheritdoc />
  public async Task RemovePasskeyAsync(TUser user, byte[] credentialId, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    
    user.Passkeys.RemoveAll(x => x.CredentialId.SequenceEqual(credentialId));
    await Db.Update(user, cancellationToken);
  }
  
  
  /// <summary>
  /// Called to create a new instance of a <see cref="UserPasskey"/>.
  /// </summary>
  /// <param name="user">The user.</param>
  /// <param name="passkey">The passkey.</param>
  /// <returns></returns>
  protected UserPasskey CreateUserPasskey(TUser user, UserPasskeyInfo passkey)
  {
    return new UserPasskey
    {
      UserId = user.Id,
      CredentialId = passkey.CredentialId,
      Data = new IdentityPasskeyData()
      {
        PublicKey = passkey.PublicKey,
        Name = passkey.Name,
        CreatedAt = passkey.CreatedAt,
        Transports = passkey.Transports,
        SignCount = passkey.SignCount,
        IsUserVerified = passkey.IsUserVerified,
        IsBackupEligible = passkey.IsBackupEligible,
        IsBackedUp = passkey.IsBackedUp,
        AttestationObject = passkey.AttestationObject,
        ClientDataJson = passkey.ClientDataJson
      }
    };
  }
  
  
  /// <summary>
  /// Find a passkey with the specified credential id for a user.
  /// </summary>
  /// <param name="userId">The user's id.</param>
  /// <param name="credentialId">The credential id to search for.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>The user passkey if it exists.</returns>
  private async Task<UserPasskey> FindUserPasskeyAsync(string userId, byte[] credentialId, CancellationToken cancellationToken)
  {
    TUser user = await Db.Find<TUser>(x => !x.IsDeleted && x.Id == userId && x.Passkeys.Any(p => p.CredentialId.SequenceEqual(credentialId)), cancellationToken); 
    return user.Passkeys.FirstOrDefault(x => x.CredentialId.SequenceEqual(credentialId));
  }

  /// <summary>
  /// Find a passkey with the specified credential id.
  /// </summary>
  /// <param name="credentialId">The credential id to search for.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
  /// <returns>The user passkey if it exists.</returns>
  private async Task<UserPasskey> FindUserPasskeyByIdAsync(byte[] credentialId, CancellationToken cancellationToken)
  {
    TUser user = await Db.Find<TUser>(x => !x.IsDeleted && x.Passkeys.Any(p => p.CredentialId.SequenceEqual(credentialId)), cancellationToken); 
    return user.Passkeys.FirstOrDefault(x => x.CredentialId.SequenceEqual(credentialId));
  }
}
