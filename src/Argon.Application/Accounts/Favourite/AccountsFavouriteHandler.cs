namespace Argon.Application.Accounts.Favourite;

[UsedImplicitly]
public class AccountsFavouriteHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<AccountsFavouriteRequest>
{
  public async Task Handle(AccountsFavouriteRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    entity.IsFavourite = request.IsFavourite;

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
