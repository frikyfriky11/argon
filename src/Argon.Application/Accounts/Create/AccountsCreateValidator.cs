namespace Argon.Application.Accounts.Create;

[UsedImplicitly]
public class AccountsCreateValidator : AbstractValidator<AccountsCreateRequest>
{
  public AccountsCreateValidator()
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}