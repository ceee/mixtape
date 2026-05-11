using Raven.Client.Documents.Indexes;

namespace Mixtape.Raven;

public class MixtapeTreeHierarchyIndexResult : MixtapeIdEntity, ISupportsDbConventions
{
  public List<string> Path { get; set; } = [];
}

public abstract class MixtapeTreeHierarchyIndex<T> : MixtapeIndex<T, MixtapeTreeHierarchyIndexResult> where T : MixtapeIdEntity, ISupportsTrees
{
  protected override void Create()
  {
    Map = items => items.Select(item => new MixtapeTreeHierarchyIndexResult
    {
      Id = item.Id,
      Path = Recurse(item, x => LoadDocument<T>(x.ParentId))
        .Where(x => x != null && x.Id != null && x.Id != item.Id)
        .Reverse()
        .Select(current => current.Id)
        .ToList()
    });

    StoreAllFields(FieldStorage.Yes);
    //Index(x => x.ChannelId, FieldIndexing.Exact);
  }
}