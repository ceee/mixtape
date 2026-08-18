namespace Mixtape.Extensions;

public static class EnumerableExtensions
{
  public static IEnumerable<T> Paging<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
  {
    pageNumber = pageNumber.Limit(1, 10_000_000);
    pageSize = pageSize.Limit(1, 1_000);

    if (pageNumber <= 0 || pageSize <= 0)
    {
      throw new NotSupportedException("Both pageNumber and pageSize must be greater than z_ero");
    }

    return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
  }
  
  
  public static IQueryable<T> Paging<T>(this IQueryable<T> source, int pageNumber, int pageSize)
  {
    pageNumber = pageNumber.Limit(1, 10_000_000);
    pageSize = pageSize.Limit(1, 1_000);

    if (pageNumber <= 0 || pageSize <= 0)
    {
      throw new NotSupportedException("Both pageNumber and pageSize must be greater than z_ero");
    }

    return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
  }


  public static bool TryGet<T>(this IEnumerable<T> source, Func<T, bool> predicate, out T model)
  {
    model = source.FirstOrDefault(predicate);
    return model != null;
  }


  public static bool TryAdd<T>(this IList<T> source, T model) where T : class
  {
    if (model == default)
    {
      return false;
    }

    source.Add(model);
    return true;
  }
}