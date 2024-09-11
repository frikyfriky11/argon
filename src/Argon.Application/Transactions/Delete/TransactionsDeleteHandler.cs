namespace Argon.Application.Transactions.Delete;

[UsedImplicitly]
public class TransactionsDeleteHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<TransactionsDeleteRequest>
{
  public async Task Handle(TransactionsDeleteRequest request, CancellationToken cancellationToken)
  {
    Transaction? entity = await dbContext
      .Transactions
      .Where(transaction => transaction.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Transaction), request.Id);
    }

    dbContext.Transactions.Remove(entity);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
