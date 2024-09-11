namespace Argon.Application.Accounts.Get;

[UsedImplicitly]
public class AccountsGetHandler : IRequestHandler<AccountsGetRequest, AccountsGetResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsGetHandler(IApplicationDbContext dbContext, IMapper mapper)
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
