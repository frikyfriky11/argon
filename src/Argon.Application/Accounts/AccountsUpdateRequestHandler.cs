namespace Argon.Application.Accounts;

/// <summary>
///   The request to update an existing account
/// </summary>
/// <param name="Name">The name of the account</param>
/// <param name="Type">The type of the account</param>
[PublicAPI]
public record AccountsUpdateRequest(string Name, AccountType Type) : IRequest
{
  /// <summary>
  ///   This field is used only internally to manually bind the [FromRoute] Guid id attribute.
  ///   It is not displayed in the documentation because the user of the API should use the route parameter.
  ///   This cannot be made internal because it would cause conflicts since you couldn't ever set it.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid Id { get; set; }
}

[UsedImplicitly]
public class AccountsUpdateRequestValidator : AbstractValidator<AccountsUpdateRequest>
{
  public AccountsUpdateRequestValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}

[UsedImplicitly]
public class AccountsUpdateRequestHandler : IRequestHandler<AccountsUpdateRequest>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public AccountsUpdateRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
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
