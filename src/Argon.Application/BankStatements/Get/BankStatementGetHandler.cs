using Argon.Application.Transactions.GetList;

namespace Argon.Application.BankStatements.Get;

[UsedImplicitly]
public class BankStatementGetHandler(
  IApplicationDbContext dbContext,
  IEnumerable<IParser> parsers
) : IRequestHandler<BankStatementGetRequest, BankStatementGetResponse>
{
  public async Task<BankStatementGetResponse> Handle(BankStatementGetRequest request, CancellationToken cancellationToken)
  {
    var result = await dbContext
      .BankStatements
      .AsNoTracking()
      .Where(bs => bs.Id == request.Id)
      .Select(bs => new
      {
        BankStatement = bs,
        Transactions = bs.Transactions
          .OrderByDescending(transaction => transaction.Date)
          .ThenByDescending(transaction => transaction.Created)
          .ThenByDescending(transaction => transaction.Id)
          .Select(transaction =>
            new TransactionsGetListResponse(
              transaction.Id,
              transaction.Date,
              transaction.CounterpartyId,
              transaction.Counterparty != null ? transaction.Counterparty.Name : string.Empty,
              transaction.TransactionRows.Select(row =>
                new TransactionRowsGetListResponse(
                  row.Id,
                  row.RowCounter,
                  row.AccountId,
                  row.Account != null ? row.Account.Name : string.Empty,
                  row.Debit,
                  row.Credit,
                  row.Description
                )).ToList(),
              transaction.RawImportData,
              transaction.Status,
              transaction.PotentialDuplicateOfTransactionId
            ))
          .ToList(),
      })
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException(nameof(BankStatement), request.Id);
    }

    return new BankStatementGetResponse(
      result.BankStatement.Id,
      result.BankStatement.FileName,
      parsers.First(p => p.ParserId == result.BankStatement.ParserId).ParserDisplayName,
      result.Transactions
    );
  }
}