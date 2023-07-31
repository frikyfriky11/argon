namespace Argon.Application.Common.Models;

/// <summary>
///   This object represents a typical paginated list request that must be derived to create actual requests
/// </summary>
/// <param name="PageNumber">The number of the page to retrieve from the data source</param>
/// <param name="PageSize">The number of items in the page that must be retrieved from the data source</param>
public abstract record PaginatedListRequest(int PageNumber, int PageSize);
