using System.Linq.Expressions;

namespace Mixtape.Raven;

public class RavenOptions
{
  public string Url { get; set; }

  public string Database { get; set; }

  public string CollectionPrefix { get; set; } = string.Empty;

  public int CacheInMinutes { get; set; } = 60;

  public RavenIndexesOptions Indexes { get; set; } = new();
}


public class RavenIndexesOptions : List<RavenIndexesOptions.Map>
{
  public class Map
  {
    internal Type Type { get; set; }

    internal Expression<Func<IMixtapeIndexDefinition>> CreateIndex { get; set; }

    internal Map(Type type, Expression<Func<IMixtapeIndexDefinition>> create)
    {
      Type = type;
      CreateIndex = create;
    }
  }


  public RavenIndexModifiersOptions Modifiers { get; private set; } = [];

  public void Add<T>() where T : IMixtapeIndexDefinition, new()
  {
    base.Add(new Map(typeof(T), () => new T()));
  }

  public void Add(Type indexType)
  {
    base.Add(new Map(indexType, () => (IMixtapeIndexDefinition)Activator.CreateInstance(indexType)));
  }

  public void Add<T>(T index) where T : IMixtapeIndexDefinition
  {
    base.Add(new Map(typeof(T), () => index));
  }

  public void AddRange(params Type[] indexes)
  {
    foreach (Type type in indexes)
    {
      Add(type);
    }
  }

  public void Replace<T, TReplaceWith>()
      where T : IMixtapeIndexDefinition, new()
      where TReplaceWith : IMixtapeIndexDefinition, new()
  {
    Replace(typeof(T), typeof(TReplaceWith));
  }

  public void Replace(Type origin, Type replaceWith)
  {
    var item = this.FirstOrDefault(x => x.Type == origin);
    if (item != null)
    {
      Remove(item);
    }
    Add(replaceWith);
  }

  public IEnumerable<IMixtapeIndexDefinition> BuildAll(IMixtapeOptions options, IDocumentStore store)
  {
    RavenOptions ravenOptions = options.For<RavenOptions>();

    foreach (Map map in this)
    {
      IMixtapeIndexDefinition index = map.CreateIndex.Compile().Invoke();
      index.Setup(options, store);
      index.RunModifiers(ravenOptions);
      yield return index;
    }
  }
}


public class RavenIndexModifiersOptions : List<RavenIndexModifiersOptions.Modifier>
{
  public class Modifier
  {
    public Type Type { get; set; }

    public Expression<Action<IMixtapeIndexDefinition>> Modify { get; set; }
  }

  public void Add<T>(Action<T> modify) where T : IMixtapeIndexDefinition, new()
  {
    Add(new()
    {
      Type = typeof(T),
      Modify = x => modify((T)x)
    });
  }


  public IEnumerable<Modifier> GetAllForType<T>() where T : IMixtapeIndexDefinition, new() => GetAllForType(typeof(T));


  public IEnumerable<Modifier> GetAllForType(Type type)
  {
    foreach (Modifier modifier in this.Where(x => x.Type.IsAssignableFrom(type)))
    {
      yield return modifier;
    }
  }
}
