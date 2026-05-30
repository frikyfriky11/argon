namespace Argon.Application.Accounts.Update;

[UsedImplicitly]
public class AccountsUpdateValidator : AbstractValidator<AccountsUpdateRequest>
{
  public AccountsUpdateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(50)
      .MustAsync(async (request, name, cancellationToken) =>
        !await dbContext.Accounts.AnyAsync(account => account.Id != request.Id && account.Name.ToLower() == name.ToLower(), cancellationToken))
      .WithMessage("An account named '{PropertyValue}' already exists");

    RuleFor(request => request.Type)
      .IsInEnum();
  }
}