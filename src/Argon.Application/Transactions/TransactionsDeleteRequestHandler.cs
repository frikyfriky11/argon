namespace Argon.Application.Transactions;

/// <summary>
///   The request to delete an existing Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
[PublicAPI]
public record TransactionsDeleteRequest(Guid Id) : IRequest;

[UsedImplicitly]
public class TransactionsDeleteRequestHandler : IRequestHandler<TransactionsDeleteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public TransactionsDeleteRequestHandler(IApplicationDbContext dbContext)
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
