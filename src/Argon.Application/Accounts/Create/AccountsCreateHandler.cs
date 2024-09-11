namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateHandler : IRequestHandler<AccountsCreateRequest, AccountsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsCreateHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<AccountsCreateResponse> Handle(AccountsCreateRequest request, CancellationToken cancellationToken)
  {
    Account entity = _mapper.Map<Account>(request);

    await _dbContext.Accounts.AddAsync(entity, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new AccountsCreateResponse(entity.Id);
  }
}
