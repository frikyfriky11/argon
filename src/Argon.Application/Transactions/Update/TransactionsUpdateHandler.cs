namespace Argon.Application.Transactions.Update;

[UsedImplicitly]
public class TransactionsUpdateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<TransactionsUpdateRequest>
{
  public async Task Handle(TransactionsUpdateRequest request, CancellationToken cancellationToken)
  {
    Transaction? entity = await dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .Where(transaction => transaction.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Transaction), request.Id);
    }

    entity.CounterpartyId = request.CounterpartyId;
    entity.Date = request.Date;

    List<TransactionRowsUpdateRequest> tempRequestRows = request.TransactionRows;

    foreach (TransactionRow entityRow in entity.TransactionRows)
    {
      TransactionRowsUpdateRequest? requestRow = tempRequestRows.FirstOrDefault(r => r.Id == entityRow.Id);

      if (requestRow is not null)
      {
        entityRow.AccountId = requestRow.AccountId;
        entityRow.RowCounter = requestRow.RowCounter;
        entityRow.Description = requestRow.Description;
        entityRow.Debit = requestRow.Debit;
        entityRow.Credit = requestRow.Credit;
        
        tempRequestRows.Remove(requestRow);
      }
      else
      {
        entity.TransactionRows.Remove(entityRow);
      }
    }

    foreach (TransactionRowsUpdateRequest requestRow in tempRequestRows)
    {
      entity.TransactionRows.Add(new TransactionRow
      {
        AccountId = requestRow.AccountId,
        RowCounter = requestRow.RowCounter,
        Description = requestRow.Description,
        Debit = requestRow.Debit,
        Credit = requestRow.Credit,
      });
    }

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
