namespace Argon.Application.Accounts;

/// <summary>
///   The request to get an existing Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
[PublicAPI]
public record AccountsGetRequest(Guid Id) : IRequest<AccountsGetResponse>;

/// <summary>
///   The result of the get request of a Account entity
/// </summary>
/// <param name="Id">The id of the account</param>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
/// <param name="IsFavourite">Whether the account is marked as favourite</param>
/// <param name="TotalAmount">The total amount that the account has registered</param>
[PublicAPI]
public record AccountsGetResponse(Guid Id, string Name, AccountType Type, bool IsFavourite, decimal TotalAmount);

[UsedImplicitly]
public class AccountsGetRequestHandler : IRequestHandler<AccountsGetRequest, AccountsGetResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsGetRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<AccountsGetResponse> Handle(AccountsGetRequest request, CancellationToken cancellationToken)
  {
    AccountsGetResponse? result = await _dbContext
      .Accounts
      .AsNoTracking()
      .Where(account => account.Id == request.Id)
      .ProjectTo<AccountsGetResponse>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
