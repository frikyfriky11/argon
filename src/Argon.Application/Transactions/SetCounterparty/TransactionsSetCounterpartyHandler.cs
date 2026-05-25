namespace Argon.Application.Transactions.SetCounterparty;

[UsedImplicitly]
public class TransactionsSetCounterpartyHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<TransactionsSetCounterpartyRequest>
{
  public async Task Handle(TransactionsSetCounterpartyRequest request, CancellationToken cancellationToken)
  {
    Transaction? transaction = await dbContext
      .Transactions
      .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);

    if (transaction is null)
    {
      throw new NotFoundException(nameof(Transaction), request.TransactionId);
    }

    transaction.CounterpartyId = request.CounterpartyId;

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
