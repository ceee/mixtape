using System.Collections.Generic;
using Fisher;

namespace Mixtape.Sqlite;

public class MixtapeStore(IDocumentStore fisher) : IMixtapeStore
{
  protected Dictionary<string, IDocumentSession> ScopedSessions { get; set; } = new();
  private const string NullDb = "__default__";


  /// <inheritdoc />
  public IDocumentStore Fisher { get; } = fisher;


  /// <inheritdoc />
  public IDocumentSession Session(MixtapeSessionResolution resolution = MixtapeSessionResolution.Reuse, SessionOptions options = null)
  {
    options ??= new();

    if (resolution == MixtapeSessionResolution.Create)
    {
      return Fisher.OpenSession(options);
    }

    if (!ScopedSessions.TryGetValue("default", out IDocumentSession session))
    {
      session = Fisher.OpenSession(options);
      ScopedSessions.TryAdd("default", session);
    }

    return session;
  }
}


public enum MixtapeSessionResolution
{
  Reuse = 0,
  Create = 1
}


public interface IMixtapeStore
{
  /// <summary>
  /// Get underlying fisher document store
  /// </summary>
  IDocumentStore Fisher { get; }

  /// <summary>
  /// Use a specific session
  /// </summary>
  IDocumentSession Session(MixtapeSessionResolution resolution = MixtapeSessionResolution.Reuse, SessionOptions options = null);
}