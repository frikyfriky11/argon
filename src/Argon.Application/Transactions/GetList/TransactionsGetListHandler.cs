using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.Transactions.GetList;

[UsedImplicitly]
public class TransactionsGetListHandler(
  IApplicationDbContext dbContext
): IRequestHandler<TransactionsGetListRequest, PaginatedList<TransactionsGetListResponse>>
{
  public async Task<PaginatedList<TransactionsGetListResponse>> Handle(TransactionsGetListRequest request, CancellationToken cancellationToken)
  {
    return await dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => request.AccountIds == null || request.AccountIds.Count == 0 || transaction.TransactionRows.Any(row => row.AccountId != null && request.AccountIds.Contains(row.AccountId.Value)))
      .Where(transaction => request.CounterpartyIds == null || request.CounterpartyIds.Count == 0 || transaction.CounterpartyId != null && request.CounterpartyIds.Contains(transaction.CounterpartyId.Value))
      .Where(transaction => request.DateFrom == null || transaction.Date >= DateOnly.FromDateTime(request.DateFrom.Value.Date))
      .Where(transaction => request.DateTo == null || transaction.Date <= DateOnly.FromDateTime(request.DateTo.Value.Date))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.CounterpartyId,
        transaction.Counterparty.Name,
        transaction.TransactionRows
          .OrderBy(row => row.RowCounter)
          .ThenBy(row => row.Id)
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList(),
        transaction.RawImportData,
        transaction.Status,
        transaction.PotentialDuplicateOfTransactionId
      ))
      .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    
    // order of the records must be deterministic and avoid random sorting
    // when two or more records have the same Date, so that when pagination occurs
    // no record is skipped. sorting by Id is sufficient because it is a primary key
    // and thus is unique, but adding the Created field shows the transaction in the
    // order they were entered too, so that's a bonus
  }
}
