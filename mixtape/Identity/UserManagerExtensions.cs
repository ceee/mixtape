using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Mixtape.Rendering.QrCode;

namespace Mixtape.Identity;

public static class UserManagerExtensions
{
  private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits={3}&algorithm={4}&period={5}";


  /// <summary>
  /// Adds an implementation of identity information stores.
  /// </summary>
  public static Task<TwoFactorKey> GetTwoFactorKey<T>(this UserManager<T> userManager, T user, string issuer)
    where T : class => GetTwoFactorKey(userManager, user, new TwoFactorKeyOptions() { Issuer = issuer });


  /// <summary>
  /// Adds an implementation of identity information stores.
  /// </summary>
  public static async Task<TwoFactorKey> GetTwoFactorKey<T>(this UserManager<T> userManager, T user, TwoFactorKeyOptions options)
    where T : class
  {
    string unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
    string email = await userManager.GetEmailAsync(user);
    
    if (unformattedKey.IsNullOrEmpty())
    {
      await userManager.ResetAuthenticatorKeyAsync(user);
      unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
    }

    string url = string.Format(
      CultureInfo.InvariantCulture,
      AuthenticatorUriFormat,
      UrlEncoder.Default.Encode(options.Issuer),
      UrlEncoder.Default.Encode(email),
      unformattedKey,
      (int)options.Digits,
      options.Algorithm.ToString(),
      (int)options.Period);
    
    QrCode qr = QrCode.EncodeText(url, QrCode.Ecc.Medium);

    return new TwoFactorKey()
    {
      UserEmail = email,
      UnformattedKey = unformattedKey,
      FormattedKey = FormatTwoFactorAuthenticationKey(unformattedKey),
      AuthenticatorUrl = url,
      QrCode = qr,
      Options = options
    };
  }


  public static async Task<Result<string[]>> SetupTwoFactorAuth<T>(this UserManager<T> userManager, T user,
    string inputCode, int recoveryCodesToGenerate = 10) where T : class
  {
    string token = inputCode.Replace(" ", string.Empty).Replace("-", string.Empty);
    bool is2FaTokenValid = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, token);

    if (!is2FaTokenValid)
    {
      return Result<string[]>.Fail();
    }
    
    // enable two-factor auth for account
    await userManager.SetTwoFactorEnabledAsync(user, true);

    // generate recovery codes
    if (await userManager.CountRecoveryCodesAsync(user) == 0)
    {
      IEnumerable<string> recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, recoveryCodesToGenerate);
      return Result<string[]>.Success(recoveryCodes.ToArray());
    }

    return Result<string[]>.Success([]);
  }


  private static string FormatTwoFactorAuthenticationKey(string unformattedKey)
  {
    var result = new StringBuilder();
    int currentPosition = 0;
    while (currentPosition + 4 < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
      currentPosition += 4;
    }
    if (currentPosition < unformattedKey.Length)
    {
      result.Append(unformattedKey.AsSpan(currentPosition));
    }

    return result.ToString().ToLowerInvariant();
  }
}