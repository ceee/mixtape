using Raven.Client.Documents.Linq;
using System.Linq.Expressions;

namespace Mixtape.Raven;

public static class RavenQueryableExtensions
{
  public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string path, bool isDescending, OrderingType type = OrderingType.String)
  {
    if (!string.IsNullOrEmpty(path))
    {
      char[] a = path.ToCharArray();
      a[0] = char.ToUpper(a[0]);
      path = new string(a);
    }

    if (isDescending)
    {
      return source.OrderByDescending(path, type);
    }
    return source.OrderBy(path, type);
  }


  public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string path, bool isDescending, OrderingType type = OrderingType.String)
  {
    if (!string.IsNullOrEmpty(path))
    {
      char[] a = path.ToCharArray();
      a[0] = char.ToUpper(a[0]);
      path = new string(a);
    }

    if (isDescending)
    {
      return source.ThenByDescending(path, type);
    }
    return source.ThenBy(path, type);
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


  public static IRavenQueryable<T> WhereIf<T>(this IRavenQueryable<T> source, Expression<Func<T, bool>> predicate, bool condition, Expression<Func<T, bool>> elsePredicate = null)
  {
    if (!condition)
    {
      if (elsePredicate != null)
      {
        return source.Where(elsePredicate);
      }
      return source;
    }

    return source.Where(predicate);
  }


  public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, bool condition, Expression<Func<T, bool>> elsePredicate = null)
  {
    if (!condition)
    {
      if (elsePredicate != null)
      {
        return source.Where(elsePredicate);
      }
      return source;
    }

    return source.Where(predicate);
  }


  public static IQueryable<T> SearchIf<T>(this IQueryable<T> source, Expression<Func<T, object>> fieldSelector, string searchTerms, string suffix = null, string prefix = null, SearchOperator @operator = SearchOperator.Or)
  {
    if (string.IsNullOrWhiteSpace(searchTerms))
    {
      return source;
    }

    searchTerms = searchTerms.Trim();

    string[] searchParts = searchTerms.Split([" "], StringSplitOptions.RemoveEmptyEntries).Select(x =>
    {
      if (suffix != null)
      {
        x += suffix;
      }
      if (prefix != null)
      {
        x = prefix + x;
      }
      return x;
    }).ToArray();

    if (searchTerms.StartsWith('"') && searchTerms.EndsWith('"'))
    {
      searchParts = [searchTerms];
    }

    return source.Search(fieldSelector, searchParts, @operator: @operator);
  }
}
