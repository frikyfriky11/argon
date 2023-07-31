namespace Argon.Application.Accounts;

/// <summary>
///   The request to get a list of Account entities
/// </summary>
[PublicAPI]
public record AccountsGetListRequest : IRequest<List<AccountsGetListResponse>>;

/// <summary>
///   The result of the Account entities get list
/// </summary>
/// <param name="Id">The id of the account</param>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
/// <param name="TotalAmount">The total amount that the account has registered</param>
[PublicAPI]
public record AccountsGetListResponse(Guid Id, string Name, AccountType Type, decimal TotalAmount);

[UsedImplicitly]
public class AccountsGetListRequestHandler : IRequestHandler<AccountsGetListRequest, List<AccountsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsGetListRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<List<AccountsGetListResponse>> Handle(AccountsGetListRequest request, CancellationToken cancellationToken)
  {
    return await _dbContext
      .Accounts
      .AsNoTracking()
      .OrderBy(account => account.Name)
      .ProjectTo<AccountsGetListResponse>(_mapper.ConfigurationProvider)
      .ToListAsync(cancellationToken);
  }
}
