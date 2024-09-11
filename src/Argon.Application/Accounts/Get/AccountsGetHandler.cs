namespace Argon.Application.Accounts.Get;

[UsedImplicitly]
public class AccountsGetHandler(
  IApplicationDbContext dbContext
): IRequestHandler<AccountsGetRequest, AccountsGetResponse>
{
  public async Task<AccountsGetResponse> Handle(AccountsGetRequest request, CancellationToken cancellationToken)
  {
    AccountsGetResponse? result = await dbContext
      .Accounts
      .AsNoTracking()
      .Where(account => account.Id == request.Id)
      .Select(account => new AccountsGetResponse(
        account.Id,
        account.Name,
        account.Type,
        account.IsFavourite,
        account.TransactionRows.Sum(x => (x.Debit ?? 0) - (x.Credit ?? 0))
      ))
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
