namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateValidator : AbstractValidator<AccountsCreateRequest>
{
  public AccountsCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}