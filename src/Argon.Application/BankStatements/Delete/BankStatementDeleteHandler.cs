namespace Argon.Application.BankStatements.Delete;

[UsedImplicitly]
public class BankStatementDeleteHandler(
  IApplicationDbContext dbContext
)
  : IRequestHandler<BankStatementDeleteRequest>
{
  public async Task Handle(BankStatementDeleteRequest request, CancellationToken cancellationToken)
  {
    BankStatement? bankStatement = await dbContext
      .BankStatements
      .Include(bs => bs.Transactions)
      .FirstOrDefaultAsync(bs => bs.Id == request.Id, cancellationToken);

    if (bankStatement is null)
    {
      throw new NotFoundException(nameof(BankStatement), request.Id);
    }

    // Manually delete associated transactions first, as the relationship is optional and won't cascade automatically
    dbContext.Transactions.RemoveRange(bankStatement.Transactions);

    dbContext.BankStatements.Remove(bankStatement);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}