using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace Mixtape.Tokens;

public class MixtapeTokenProvider(IMixtapeTokenStoreDbProvider db) : IMixtapeTokenProvider
{
  readonly char[] _chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-".ToCharArray();

  readonly RandomNumberGenerator _randonNumberGenerator = RandomNumberGenerator.Create();

  protected IMixtapeTokenStoreDbProvider Db { get; } = db;


  /// <inheritdoc />
  public virtual async Task<string> Create(string key, TimeSpan expires, int length = 82, Dictionary<string, string> metadata = null)
  {
    if (key.IsNullOrWhiteSpace())
    {
      throw new ArgumentException("Key cannot be empty");
    }
    
    ArgumentOutOfRangeException.ThrowIfLessThan(length, 16);

    string tokenKey = Random(length);
    DateTimeOffset expiresAt = DateTimeOffset.Now.Add(expires);

    SecurityToken securityToken = new()
    {
      Id = TokenToId(tokenKey),
      Token = tokenKey,
      Key = HashKey(key),
      Metadata = metadata ?? new(),
      Expires = expiresAt
    };

    // saves the token
    await Db.Create(securityToken);
    // set expiration?

    return tokenKey;
  }


  /// <inheritdoc />
  public virtual async Task<bool> Verify(string key, string token)
  {
    return await VerifyAndReturn(key, token) != null;
  }


  /// <inheritdoc />
  public virtual async Task<SecurityToken> VerifyAndReturn(string key, string token)
  {
    if (token.IsNullOrWhiteSpace() || key.IsNullOrWhiteSpace())
    {
      return null;
    }

    // try to find a valid token
    SecurityToken securityToken = await Db.Load<SecurityToken>(TokenToId(token));;

    if (securityToken == null)
    {
      return null;
    }

    // check if token is expired
    if (securityToken.Expires < DateTimeOffset.Now)
    {
      await Db.Delete(securityToken);
      return null;
    }

    // verify cryptographic key
    if (!VerifyKey(securityToken.Key, key))
    {
      return null;
    }

    // remove token from DB if it is valid
    await Db.Delete(securityToken);
    return securityToken;
  }


  /// <inheritdoc />
  public string Random(int length)
  {
    byte[] data = new byte[4 * length];
    _randonNumberGenerator.GetBytes(data);

    StringBuilder result = new(length);

    for (int i = 0; i < length; i++)
    {
      uint rnd = BitConverter.ToUInt32(data, i * 4);
      long idx = rnd % _chars.Length;

      result.Append(_chars[idx]);
    }

    return result.ToString();
  }


  /// <inheritdoc />
  public virtual async Task<bool> Exists(string token)
  {
    if (token.IsNullOrWhiteSpace())
    {
      return false;
    }
    
    // try to find a valid token
    SecurityToken securityToken = await Db.Load<SecurityToken>(TokenToId(token));
    return securityToken != null;
  }


  /// <summary>
  /// Converts the token to a database id
  /// </summary>
  string TokenToId(string token)
  {
    return $"sectok.{token}";
  }


  /// <summary>
  /// Creates a hash for the security token key.
  /// Borrowed from the .NET Core PasswordHasher 
  /// (see: https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Identity/Extensions.Core/src/PasswordHasher.cs)
  /// </summary>
  string HashKey(string key)
  {
    key = key.ToLowerInvariant();

    const KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256;
    const int iterationCount = 10000;
    const int saltSize = 128 / 8;
    const int numBytesRequested = 256 / 8;

    byte[] salt = new byte[saltSize];
    _randonNumberGenerator.GetBytes(salt);
    byte[] subkey = KeyDerivation.Pbkdf2(key, salt, prf, iterationCount, numBytesRequested);

    byte[] outputBytes = new byte[13 + salt.Length + subkey.Length];
    outputBytes[0] = 0x01;
    WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
    WriteNetworkByteOrder(outputBytes, 5, (uint)iterationCount);
    WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
    Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
    Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
    return Convert.ToBase64String(outputBytes);
  }


  /// <summary>
  /// Verifies the given key.
  /// Borrowed from the .NET Core PasswordHasher 
  /// (see: https://github.com/dotnet/aspnetcore/blob/release/5.0/src/Identity/Extensions.Core/src/PasswordHasher.cs)
  /// </summary>
  bool VerifyKey(string storedKey, string key)
  {
    byte[] decodedStoredKey = Convert.FromBase64String(storedKey);

    try
    {
      KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(decodedStoredKey, 1);
      int embeddedIterCount = (int)ReadNetworkByteOrder(decodedStoredKey, 5);
      int saltLength = (int)ReadNetworkByteOrder(decodedStoredKey, 9);

      // Read the salt: must be >= 128 bits
      if (saltLength < 128 / 8)
      {
        return false;
      }

      byte[] salt = new byte[saltLength];
      Buffer.BlockCopy(decodedStoredKey, 13, salt, 0, salt.Length);

      // Read the subkey (the rest of the payload): must be >= 128 bits
      int subkeyLength = decodedStoredKey.Length - 13 - salt.Length;
      if (subkeyLength < 128 / 8)
      {
        return false;
      }
      byte[] expectedSubkey = new byte[subkeyLength];
      Buffer.BlockCopy(decodedStoredKey, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

      // Hash the incoming key and verify it
      byte[] actualSubkey = KeyDerivation.Pbkdf2(key, salt, prf, embeddedIterCount, subkeyLength);

      return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }
    catch
    {
      return false;
    }
  }


  static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
  {
    buffer[offset + 0] = (byte)(value >> 24);
    buffer[offset + 1] = (byte)(value >> 16);
    buffer[offset + 2] = (byte)(value >> 8);
    buffer[offset + 3] = (byte)(value >> 0);
  }

  static uint ReadNetworkByteOrder(byte[] buffer, int offset)
  {
    return ((uint)(buffer[offset + 0]) << 24)
      | ((uint)(buffer[offset + 1]) << 16)
      | ((uint)(buffer[offset + 2]) << 8)
      | ((uint)(buffer[offset + 3]));
  }
}


public interface IMixtapeTokenProvider
{
  /// <summary>
  /// Generates a token for a <paramref name="key"/> with a specified <paramref name="expires"/> lifespan.
  /// </summary>
  /// <param name="key">The purpose the token will be used for. The key must match when verifying the token and should always be hidden from the user.</param>
  /// <param name="expires">The token will automatically expire after this timespan.</param>
  /// <param name="length">Length of the geneated token.</param> 
  /// <param name="metadata">Additional metadata to store with the token. This data can be retrieved on verification with <see cref="VerifyAndReturn(string, string)"/></param>
  /// <returns>
  /// The generated token with the specified <paramref name="length"/>.
  /// </returns>
  Task<string> Create(string key, TimeSpan expires, int length = 82, Dictionary<string, string> metadata = null);

  /// <summary>
  /// Validates the passed <paramref name="token"/> for the specified <paramref name="key"/>.
  /// </summary>
  /// <param name="key">The purpose the token was used for.</param>
  /// <param name="token">The previously generated token.</param>
  /// <returns>
  /// False, if the token could not be found or it has already expired.
  /// </returns>
  Task<bool> Verify(string key, string token);

  /// <summary>
  /// Validates the passed <paramref name="token"/> for the specified <paramref name="key"/>.
  /// </summary>
  /// <param name="key">The purpose the token was used for.</param>
  /// <param name="token">The previously generated token.</param>
  /// <returns>
  /// Null, if the token could not be found or it has already expired.
  /// </returns>
  Task<SecurityToken> VerifyAndReturn(string key, string token);

  /// <summary>
  /// Generates a random token with the specified <paramref name="length"/> using the RNGCryptoServiceProvider.
  /// </summary>
  /// <remarks>
  /// This method won't store the token in the database and can't be used for verification. Use <see cref="Create(string, TimeSpan, int)"/> instead.
  /// </remarks>
  string Random(int length);

  /// <summary>
  /// Determines whether a token exists in the database.
  /// </summary>
  Task<bool> Exists(string token);
}
