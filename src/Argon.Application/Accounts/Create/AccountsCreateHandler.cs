namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<AccountsCreateRequest, AccountsCreateResponse>
{
  public async Task<AccountsCreateResponse> Handle(AccountsCreateRequest request, CancellationToken cancellationToken)
  {
    Account entity = new()
    {
      Name = request.Name,
      Type = request.Type,
    };

    await dbContext.Accounts.AddAsync(entity, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new AccountsCreateResponse(entity.Id);
  }
}