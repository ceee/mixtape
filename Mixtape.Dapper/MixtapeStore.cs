namespace Mixtape.Sqlite;

public class MixtapeStore(IDocumentSession scopedSession) : IMixtapeStore
{
  /// <inheritdoc />
  public IDocumentSession Session(MixtapeSessionResolution resolution = MixtapeSessionResolution.Reuse)
  {
    return resolution == MixtapeSessionResolution.Create ? scopedSession.New() : scopedSession;
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
  /// Use a specific session
  /// </summary>
  IDocumentSession Session(MixtapeSessionResolution resolution = MixtapeSessionResolution.Reuse);
}