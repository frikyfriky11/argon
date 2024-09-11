namespace Argon.Application.Accounts.GetList;

[UsedImplicitly]
public class AccountsGetListHandler : IRequestHandler<AccountsGetListRequest, List<AccountsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsGetListHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<List<AccountsGetListResponse>> Handle(AccountsGetListRequest request, CancellationToken cancellationToken)
  {
    return await _dbContext
      .Accounts
      .AsNoTracking()
      .OrderBy(account => account.Name)
      .Select(account => new AccountsGetListResponse(
        account.Id,
        account.Name,
        account.Type,
        account.IsFavourite,
        account.TransactionRows
          .Where(row => request.TotalAmountsFrom == null || row.Transaction.Date >= DateOnly.FromDateTime(request.TotalAmountsFrom.Value.Date))
          .Where(row => request.TotalAmountsTo == null || row.Transaction.Date <= DateOnly.FromDateTime(request.TotalAmountsTo.Value.Date))
          .Sum(row => (row.Debit ?? 0) - (row.Credit ?? 0))
      ))
      .ToListAsync(cancellationToken);
  }
}
