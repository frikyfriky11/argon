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
    decimal tolerance = request.RowAmountTolerance ?? 0m;
    decimal lowerBound = (request.RowAmount ?? 0m) - tolerance;
    decimal upperBound = (request.RowAmount ?? 0m) + tolerance;
    DateOnly? dateFrom = request.DateFrom == null ? null : DateOnly.FromDateTime(request.DateFrom.Value.Date);
    DateOnly? dateTo = request.DateTo == null ? null : DateOnly.FromDateTime(request.DateTo.Value.Date);

    IQueryable<Transaction> query = dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => request.AccountIds == null || request.AccountIds.Count == 0 || transaction.TransactionRows.Any(row => row.AccountId != null && request.AccountIds.Contains(row.AccountId.Value)))
      .Where(transaction => request.CounterpartyIds == null || request.CounterpartyIds.Count == 0 || transaction.CounterpartyId != null && request.CounterpartyIds.Contains(transaction.CounterpartyId.Value))
      .Where(transaction => request.Status == null || transaction.Status == request.Status.Value)
      .Where(transaction => request.Linked == null
        || (request.Linked.Value ? transaction.CounterpartyId != null : transaction.CounterpartyId == null))
      .Where(transaction => request.RowAmount == null || transaction.TransactionRows.Any(row =>
        (row.Debit != null && row.Debit >= lowerBound && row.Debit <= upperBound)
        || (row.Credit != null && row.Credit >= lowerBound && row.Credit <= upperBound)));

    // AccountingDate filtering falls back to Date for transactions without one (manual entries).
    if (request.DateField == TransactionDateField.AccountingDate)
    {
      query = query
        .Where(transaction => dateFrom == null || (transaction.AccountingDate ?? transaction.Date) >= dateFrom)
        .Where(transaction => dateTo == null || (transaction.AccountingDate ?? transaction.Date) <= dateTo);
    }
    else
    {
      query = query
        .Where(transaction => dateFrom == null || transaction.Date >= dateFrom)
        .Where(transaction => dateTo == null || transaction.Date <= dateTo);
    }

    return await query
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.AccountingDate,
        transaction.CounterpartyId,
        transaction.Counterparty != null ? transaction.Counterparty.Name : string.Empty,
        transaction.TransactionRows
          .OrderBy(row => row.RowCounter)
          .ThenBy(row => row.Id)
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account != null ? row.Account.Name : null,
            row.Account != null ? row.Account.Type : null,
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
