namespace Argon.Application.Transactions.Get;

[UsedImplicitly]
public class TransactionsGetHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<TransactionsGetRequest, TransactionsGetResponse>
{
  public async Task<TransactionsGetResponse> Handle(TransactionsGetRequest request, CancellationToken cancellationToken)
  {
    TransactionsGetResponse? result = await dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => transaction.Id == request.Id)
      .Select(transaction => new TransactionsGetResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .OrderBy(row => row.RowCounter)
          .ThenBy(row => row.Id)
          .Select(row => new TransactionRowsGetResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
