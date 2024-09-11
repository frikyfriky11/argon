namespace Argon.Application.Accounts.Favourite;

[UsedImplicitly]
public class AccountsFavouriteHandler : IRequestHandler<AccountsFavouriteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsFavouriteHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task Handle(AccountsFavouriteRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await _dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    entity.IsFavourite = request.IsFavourite;

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
