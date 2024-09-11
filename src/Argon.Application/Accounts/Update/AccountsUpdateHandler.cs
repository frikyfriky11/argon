namespace Argon.Application.Accounts.Update;

[UsedImplicitly]
public class AccountsUpdateHandler : IRequestHandler<AccountsUpdateRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsUpdateHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Handle(AccountsUpdateRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await _dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    entity.Name = request.Name;
    entity.Type = request.Type;

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
