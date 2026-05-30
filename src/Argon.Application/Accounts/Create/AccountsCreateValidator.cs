namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateValidator : AbstractValidator<AccountsCreateRequest>
{
  public AccountsCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(50)
      .MustAsync(async (name, cancellationToken) =>
        !await dbContext.Accounts.AnyAsync(account => account.Name.ToLower() == name.ToLower(), cancellationToken))
      .WithMessage("An account named '{PropertyValue}' already exists");

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}