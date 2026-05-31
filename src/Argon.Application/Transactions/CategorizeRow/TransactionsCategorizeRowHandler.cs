namespace Argon.Application.Transactions.CategorizeRow;

[UsedImplicitly]
public class TransactionsCategorizeRowHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<TransactionsCategorizeRowRequest>
{
  public async Task Handle(TransactionsCategorizeRowRequest request, CancellationToken cancellationToken)
  {
    Transaction? transaction = await dbContext
      .Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

    if (transaction is null)
    {
      throw new NotFoundException(nameof(Transaction), request.TransactionId);
    }

    TransactionRow? row = transaction.TransactionRows.FirstOrDefault(r => r.Id == request.RowId);

    if (row is null)
    {
      throw new NotFoundException(nameof(TransactionRow), request.RowId);
    }

    row.AccountId = request.AccountId;

    if (request.Description is not null)
    {
      row.Description = request.Description;
    }

    if (transaction.Status == TransactionStatus.PendingImportReview
        && transaction.TransactionRows.All(r => r.AccountId != null))
    {
      transaction.Status = TransactionStatus.Confirmed;
      transaction.PotentialDuplicateOfTransactionId = null;
    }

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
