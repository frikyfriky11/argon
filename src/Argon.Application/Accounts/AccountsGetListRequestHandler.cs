namespace Argon.Application.Accounts;

/// <summary>
///   The request to get a list of Account entities
///   <param name="TotalAmountsFrom">The start date to use to compute the total amounts</param>
///   <param name="TotalAmountsTo">The end date to use to compute the total amounts</param>
/// </summary>
[PublicAPI]
public record AccountsGetListRequest(DateTimeOffset? TotalAmountsFrom, DateTimeOffset? TotalAmountsTo) : IRequest<List<AccountsGetListResponse>>;
// TODO find out how to replace DateTimeOffset with DateOnly and how to make it go along well with NSwag

/// <summary>
///   The result of the Account entities get list
/// </summary>
/// <param name="Id">The id of the account</param>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
/// <param name="IsFavourite">Whether the account is marked as favourite</param>
/// <param name="TotalAmount">The total amount that the account has registered</param>
[PublicAPI]
public record AccountsGetListResponse(Guid Id, string Name, AccountType Type, bool IsFavourite, decimal TotalAmount);

[UsedImplicitly]
public class AccountsGetListRequestHandler : IRequestHandler<AccountsGetListRequest, List<AccountsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;

  public AccountsGetListRequestHandler(IApplicationDbContext dbContext)
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
