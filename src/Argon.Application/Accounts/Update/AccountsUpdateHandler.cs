namespace Argon.Application.Accounts.Update;

[UsedImplicitly]
public class AccountsUpdateHandler : IRequestHandler<AccountsUpdateRequest>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsUpdateHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task Handle(AccountsUpdateRequest request, CancellationToken cancellationToken)
  {
    Account? entity = await _dbContext
      .Accounts
      .Where(account => account.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Account), request.Id);
    }

    _mapper.Map(request, entity);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
