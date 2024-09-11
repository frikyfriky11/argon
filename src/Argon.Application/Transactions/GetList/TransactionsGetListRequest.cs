using Argon.Application.Common.Models;

namespace Argon.Application.Transactions.GetList;

/// <summary>
///   The request to get a list of Transaction entities
/// </summary>
/// <param name="AccountIds">The account ids used in the transaction rows</param>
/// <param name="Description">The description used in the transaction</param>
/// <param name="DateFrom">The start date to use in the search of the transaction</param>
/// <param name="DateTo">The end date to use in the search of the transaction</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record TransactionsGetListRequest(
  List<Guid>? AccountIds,
  string? Description,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  int PageNumber = 1,
  int PageSize = 25
) : PaginatedListRequest(PageNumber, PageSize),
  IRequest<PaginatedList<TransactionsGetListResponse>>;