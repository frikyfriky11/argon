namespace Argon.Application.Accounts.Delete;

[UsedImplicitly]
public class AccountsDeleteHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<AccountsDeleteRequest>
{
  public async Task Handle(AccountsDeleteRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    dbContext.Accounts.Remove(entity);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
