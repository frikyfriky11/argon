namespace Argon.Application.Accounts;

/// <summary>
///   The request to delete an existing Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
[PublicAPI]
public record AccountsDeleteRequest(Guid Id) : IRequest;

[UsedImplicitly]
public class AccountsDeleteRequestHandler : IRequestHandler<AccountsDeleteRequest>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsDeleteRequestHandler(IApplicationDbContext dbContext)
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
