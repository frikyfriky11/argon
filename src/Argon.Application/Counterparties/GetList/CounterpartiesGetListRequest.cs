using Argon.Application.Common.Models;

namespace Argon.Application.Counterparties.GetList;

/// <summary>
///   The request to get a list of Counterparty entities
/// </summary>
/// <param name="Name">The name of the counterparties</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record CounterpartiesGetListRequest(
  string? Name,
  int PageNumber = 1,
  int PageSize = 25
) : PaginatedListRequest(PageNumber, PageSize),
  IRequest<PaginatedList<CounterpartiesGetListResponse>>;