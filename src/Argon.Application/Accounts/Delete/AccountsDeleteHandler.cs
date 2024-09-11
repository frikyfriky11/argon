namespace Argon.Application.Accounts.Delete;

[UsedImplicitly]
public class AccountsDeleteHandler : IRequestHandler<AccountsDeleteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsDeleteHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Handle(AccountsDeleteRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await _dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    _dbContext.Accounts.Remove(entity);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
