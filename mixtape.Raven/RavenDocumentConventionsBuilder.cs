using Raven.Client.Documents.Conventions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Rv = Raven.Client;

namespace Mixtape.Raven;

public class RavenDocumentConventionsBuilder : IRavenDocumentConventionsBuilder
{
  protected HashSet<Type> PolymorphTypes { get; } = [];

  protected Type AcceptsMixtapeConventionsType { get; set; } = typeof(ISupportsDbConventions);

  protected char IdentityPartsSeparator { get; set; } = '.';

  protected IMixtapeOptions Options { get; }

  protected static ConcurrentDictionary<Type, string> CachedTypeCollectionNameMap = new();


  public RavenDocumentConventionsBuilder(IMixtapeOptions options)
  {
    Options = options;
  }


  /// <inheritdoc />
  public void Run(DocumentConventions conventions)
  {
    conventions.MaxNumberOfRequestsPerSession = 100_000;
    conventions.IdentityPartsSeparator = IdentityPartsSeparator;
    conventions.TransformTypeCollectionNameToDocumentIdPrefix = TransformTypeCollectionNameToDocumentIdPrefix;
    conventions.FindCollectionName = FindCollectionName;
    conventions.RegisterAsyncIdConvention<MixtapeIdEntity>((_, entity) => GetDocumentId(conventions, entity));
  }


  /// <summary>
  /// Get a document ID from an entity
  /// </summary>
  protected virtual Task<string> GetDocumentId<T>(DocumentConventions conventions, T entity)
  {
    string collection = conventions.GetCollectionName(entity);

    StringBuilder documentId = new();
    documentId.Append(conventions.TransformTypeCollectionNameToDocumentIdPrefix(collection));
    documentId.Append(IdentityPartsSeparator);
    documentId.Append(IdGenerator.Create());

    return Task.FromResult(documentId.ToString());
  }


  /// <summary>
  /// Finds the collection name for a certain type based on internal rules
  /// </summary>
  protected virtual string FindCollectionName(Type originalType)
  {
    string collection = null;

    Type type = originalType;

    Func<string, string> cache = name =>
    {
      CachedTypeCollectionNameMap.TryAdd(type, name);
      return name;
    };

    // get inner type for revisions
    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Rv.Documents.Subscriptions.Revision<>))
    {
      type = type.GetGenericArguments().FirstOrDefault();
    }

    // try to resolve from cache
    if (CachedTypeCollectionNameMap.TryGetValue(type, out collection))
    {
      return collection;
    }

    // do not alter non-internal entities
    if (!AcceptsMixtapeConventionsType.IsAssignableFrom(type))
    {
      return cache(DocumentConventions.DefaultGetCollectionName(type));
    }

    // use name from attribute if available
    RavenCollectionAttribute collectionAttribute = type.GetCustomAttribute<RavenCollectionAttribute>(true);

    if (collectionAttribute != null)
    {
      return cache(collectionAttribute.Name);
    }

    // use base interface if available
    Type interfaceBaseType = type.GetInterfaces().FirstOrDefault(x => x.IsInterface && AcceptsMixtapeConventionsType.IsAssignableFrom(x) && x.Name != AcceptsMixtapeConventionsType.Name);

    if (interfaceBaseType != null)
    {
      // use name from attribute if available
      collectionAttribute = interfaceBaseType.GetCustomAttribute<RavenCollectionAttribute>(true);
      if (collectionAttribute != null)
      {
        return cache(collectionAttribute.Name);
      }
    }

    // use base type for polymorphism
    Type polymorphBaseType = PolymorphTypes.FirstOrDefault(x => type.IsSubclassOf(x));

    if (polymorphBaseType != null)
    {
      return cache(DocumentConventions.DefaultGetCollectionName(polymorphBaseType));
    }

    return cache(DocumentConventions.DefaultGetCollectionName(type));
  }


  /// <summary>
  /// Translates the types collection name to the document id prefix
  /// </summary>
  protected virtual string TransformTypeCollectionNameToDocumentIdPrefix(string name)
  {
    RavenOptions options = Options.For<RavenOptions>();
    if (options != null && !options.CollectionPrefix.IsNullOrWhiteSpace())
    {
      name = options.CollectionPrefix.EnsureEndsWith(IdentityPartsSeparator) + name.TrimStart(options.CollectionPrefix);
    }

    return name.ToCamelCaseId();
  }
}


public interface IRavenDocumentConventionsBuilder
{
  /// <summary>
  /// Applies internal rules to the RavenDB document conventions
  /// </summary>
  void Run(DocumentConventions conventions);
}