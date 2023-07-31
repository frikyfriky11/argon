using Argon.Application.Common.Models;

namespace Argon.Application.Extensions;

public static class QueryableExtensions
{
  /// <summary>
  ///   Transforms a data source to a PaginatedList object performing server side pagination
  /// </summary>
  /// <param name="source">An IQueryable object collection that represents the collection of items that needs to be paginated</param>
  /// <param name="pageNumber">The number of the page to retrieve from the data source</param>
  /// <param name="pageSize">The number of items in the page that must be retrieved from the data source</param>
  /// <param name="cancellationToken">A cancellation token to cancel work in progress</param>
  /// <typeparam name="TDestination">A generic type representing the collection of items returned as a paginated result set</typeparam>
  /// <returns></returns>
  public static Task<PaginatedList<TDestination>>
    PaginatedListAsync<TDestination>(this IQueryable<TDestination> source, int pageNumber, int pageSize,
      CancellationToken cancellationToken)
    where TDestination : class
  {
    return PaginatedList<TDestination>.CreateAsync(source.AsNoTracking(), pageNumber, pageSize, cancellationToken);
  }
}
