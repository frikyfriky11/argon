namespace Argon.Application.Accounts;

/// <summary>
///   The request to create a new Account entity
/// </summary>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
[PublicAPI]
public record AccountsCreateRequest(string Name, AccountType Type) : IRequest<AccountsCreateResponse>;

/// <summary>
///   The result of the creation of a new Account entity
/// </summary>
/// <param name="Id">The id of the newly created Account</param>
[PublicAPI]
public record AccountsCreateResponse(Guid Id);

[UsedImplicitly]
public class AccountsCreateRequestValidator : AbstractValidator<AccountsCreateRequest>
{
  public AccountsCreateRequestValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}

[UsedImplicitly]
public class AccountsCreateRequestHandler : IRequestHandler<AccountsCreateRequest, AccountsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsCreateRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
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
