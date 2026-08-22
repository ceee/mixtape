using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Mixtape.Utils;

public class Hashimoto
{
  /// <summary>
  /// Creates a random SHA1 hash.
  /// </summary>
  public static string Sha1() => ComputeHash(Guid.NewGuid().ToString("N"), HashAlgorithmType.Sha1);
  
  /// <summary>
  /// Creates a random SHA256 hash.
  /// </summary>
  public static string Sha256() => ComputeHash(Guid.NewGuid().ToString("N"), HashAlgorithmType.Sha256);
  
  /// <summary>
  /// Creates a random SHA512 hash.
  /// </summary>
  public static string Sha512() => ComputeHash(Guid.NewGuid().ToString("N"), HashAlgorithmType.Sha512);
  
  /// <summary>
  /// Creates a random MD5 hash.
  /// </summary>
  public static string Md5() => ComputeHash(Guid.NewGuid().ToString("N"), HashAlgorithmType.Md5);

  /// <summary>
  /// Creates a SHA1 hash from a string.
  /// </summary>
  public static string Sha1(string value) => ComputeHash(value, HashAlgorithmType.Sha1);
  
  /// <summary>
  /// Creates a SHA256 hash from a string.
  /// </summary>
  public static string Sha256(string value) => ComputeHash(value, HashAlgorithmType.Sha256);
  
  /// <summary>
  /// Creates a SHA512 hash from a string.
  /// </summary>
  public static string Sha512(string value) => ComputeHash(value, HashAlgorithmType.Sha512);
  
  /// <summary>
  /// Creates a MD5 hash from a string.
  /// </summary>
  public static string Md5(string value) => ComputeHash(value, HashAlgorithmType.Md5);
  
  /// <summary>
  /// Creates a SHA1 hash for objects.
  /// </summary>
  public static string Sha1(params object[] values) => ComputeHash(JsonSerializer.Serialize(values), HashAlgorithmType.Sha1);
  
  /// <summary>
  /// Creates a SHA256 hash for objects.
  /// </summary>
  public static string Sha256(params object[] values) => ComputeHash(JsonSerializer.Serialize(values), HashAlgorithmType.Sha256);
  
  /// <summary>
  /// Creates a SHA512 hash for objects.
  /// </summary>
  public static string Sha512(params object[] values) => ComputeHash(JsonSerializer.Serialize(values), HashAlgorithmType.Sha512);
  
  /// <summary>
  /// Creates a MD5 hash for objects.
  /// </summary>
  public static string Md5(params object[] values) => ComputeHash(JsonSerializer.Serialize(values), HashAlgorithmType.Md5);

  
  /// <summary>
  /// Computes a hash for a string.
  /// </summary>
  private static string ComputeHash(string value, HashAlgorithmType algorithm)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(value);
    byte[] hashBytes = algorithm switch
    {
      HashAlgorithmType.Sha1 => SHA1.HashData(bytes),
      HashAlgorithmType.Sha256 => SHA256.HashData(bytes),
      HashAlgorithmType.Sha512 => SHA512.HashData(bytes),
      HashAlgorithmType.Md5 => MD5.HashData(bytes),
      _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
    };

    return Convert.ToHexString(hashBytes).ToLowerInvariant();
  }
}


public enum HashAlgorithmType
{
  Sha1,
  Sha256,
  Sha512,
  Md5
}