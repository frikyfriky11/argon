using Argon.Application.Common.Models;

namespace Argon.Application.Transactions.GetList;

/// <summary>
///   The request to get a list of Transaction entities
/// </summary>
/// <param name="AccountIds">The account ids used in the transaction rows</param>
/// <param name="CounterpartyIds">The counterparty ids used in the transaction</param>
/// <param name="DateFrom">The start date to use in the search of the transaction</param>
/// <param name="DateTo">The end date to use in the search of the transaction</param>
/// <param name="Status">Filter by transaction status</param>
/// <param name="Linked">When true returns only transactions with a linked counterparty, when false only those without; null returns both</param>
/// <param name="RowAmount">When set, returns only transactions having a row whose debit or credit matches this amount (within RowAmountTolerance)</param>
/// <param name="RowAmountTolerance">The +/- tolerance applied to RowAmount (defaults to 0 = exact match)</param>
/// <param name="DateField">Which date field DateFrom/DateTo filter on (defaults to the currency Date; AccountingDate uses the booking date, falling back to Date)</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record TransactionsGetListRequest(
  List<Guid>? AccountIds,
  List<Guid>? CounterpartyIds,
  DateTimeOffset? DateFrom,
  DateTimeOffset? DateTo,
  TransactionStatus? Status = null,
  bool? Linked = null,
  decimal? RowAmount = null,
  decimal? RowAmountTolerance = null,
  TransactionDateField DateField = TransactionDateField.Date,
  int PageNumber = 1,
  int PageSize = 25
) : PaginatedListRequest(PageNumber, PageSize),
  IRequest<PaginatedList<TransactionsGetListResponse>>;