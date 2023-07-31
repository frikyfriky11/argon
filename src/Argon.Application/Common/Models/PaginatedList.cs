namespace Argon.Application.Common.Models;

/// <summary>
///   This model represents a paginated list of generic results, allowing pagination to occur for better performance when
///   retrieving large amounts of records from an endpoint.
/// </summary>
/// <typeparam name="T">A generic type representing the collection of items returned as a paginated result set</typeparam>
public class PaginatedList<T>
{
  /// <summary>
  ///   Creates a new PaginatedList object with the specified collection and page sizes
  /// </summary>
  /// <param name="items">The collection of items that this PaginatedList object represents</param>
  /// <param name="totalCount">The total count of items before pagination occurred</param>
  /// <param name="pageNumber">The number of the page representing the current subset of items</param>
  /// <param name="pageSize">The number of items in the page representing the current subset of items</param>
  public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
  {
    PageNumber = pageNumber;
    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    TotalCount = totalCount;
    Items = items;
  }

  /// <summary>
  ///   The collection of items that this PaginatedList object represents
  /// </summary>
  public List<T> Items { get; }

  /// <summary>
  ///   The number of the page representing the current subset of items
  /// </summary>
  public int PageNumber { get; }

  /// <summary>
  ///   The total number of pages that could be retrieved with the current page size
  /// </summary>
  public int TotalPages { get; }

  /// <summary>
  ///   The total count of items before pagination occurred
  /// </summary>
  public int TotalCount { get; }

  /// <summary>
  ///   Describes if there is a previous page that can be retrieved by subtracting 1 from the page number
  /// </summary>
  public bool HasPreviousPage => PageNumber > 1;

  /// <summary>
  ///   Describes if there is a next page that can be retrieved by adding 1 to the page number
  /// </summary>
  public bool HasNextPage => PageNumber < TotalPages;

  /// <summary>
  ///   Creates a new PaginatedList object starting from an IQueryable data source, with the provided page number and page
  ///   size
  /// </summary>
  /// <param name="source">An IQueryable object collection that represents the collection of items that needs to be paginated</param>
  /// <param name="pageNumber">The number of the page to retrieve from the data source</param>
  /// <param name="pageSize">The number of items in the page that must be retrieved from the data source</param>
  /// <param name="cancellationToken">A cancellation token to cancel work in progress</param>
  /// <returns>A task representing a new PaginatedList of items</returns>
  public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize,
    CancellationToken cancellationToken)
  {
    // fetch the count from the data source before paginating the results
    int count = await source.CountAsync(cancellationToken);

    List<T> items;
    
    if (pageSize == -1)
    {
      // fetch all the items from the data source without paginating them
      items = await source
        .ToListAsync(cancellationToken);

      return new PaginatedList<T>(items, count, 1, count);
    }

    // fetch the actual items from the data source paginating them
    items = await source
      .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    return new PaginatedList<T>(items, count, pageNumber, pageSize);
  }
}
