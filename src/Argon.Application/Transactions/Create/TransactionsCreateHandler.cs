namespace Argon.Application.Transactions.Create;

[UsedImplicitly]
public class TransactionsCreateHandler(
  IApplicationDbContext dbContext
): IRequestHandler<TransactionsCreateRequest, TransactionsCreateResponse>
{
  public async Task<TransactionsCreateResponse> Handle(TransactionsCreateRequest request, CancellationToken cancellationToken)
  {
    Transaction entity = new()
    {
      Description = request.Description,
      Date = request.Date,
      TransactionRows = request
        .TransactionRows
        .Select(row => new TransactionRow
        {
          Description = row.Description,
          RowCounter = row.RowCounter,
          AccountId = row.AccountId,
          Debit = row.Debit,
          Credit = row.Credit,
        })
        .ToList(),
    };

    await dbContext.Transactions.AddAsync(entity, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new TransactionsCreateResponse(entity.Id);
  }
}
