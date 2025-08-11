namespace Argon.Application.Accounts.Update;

[UsedImplicitly]
public class AccountsUpdateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<AccountsUpdateRequest>
{
  public async Task Handle(AccountsUpdateRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    entity.Name = request.Name;
    entity.Type = request.Type;

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
