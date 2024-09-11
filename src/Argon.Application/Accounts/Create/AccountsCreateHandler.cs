namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateHandler : IRequestHandler<AccountsCreateRequest, AccountsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsCreateHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<AccountsCreateResponse> Handle(AccountsCreateRequest request, CancellationToken cancellationToken)
  {
    Account entity = new()
    {
      Name = request.Name,
      Type = request.Type,
    };

    await _dbContext.Accounts.AddAsync(entity, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new AccountsCreateResponse(entity.Id);
  }
}