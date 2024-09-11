namespace Argon.Application.Transactions.Delete;

[UsedImplicitly]
public class TransactionsDeleteHandler : IRequestHandler<TransactionsDeleteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public TransactionsDeleteHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Handle(TransactionsDeleteRequest request, CancellationToken cancellationToken)
  {
    Transaction? entity = await _dbContext
      .Transactions
      .Where(transaction => transaction.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Transaction), request.Id);
    }

    _dbContext.Transactions.Remove(entity);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
