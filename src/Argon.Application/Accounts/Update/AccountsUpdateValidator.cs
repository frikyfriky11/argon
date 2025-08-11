namespace Argon.Application.Accounts.Update;

[UsedImplicitly]
public class AccountsUpdateValidator : AbstractValidator<AccountsUpdateRequest>
{
  public AccountsUpdateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(50);

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}