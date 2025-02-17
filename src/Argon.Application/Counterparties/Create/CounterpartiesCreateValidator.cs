namespace Argon.Application.Counterparties.Create;

[UsedImplicitly]
public class CounterpartiesCreateValidator : AbstractValidator<CounterpartiesCreateRequest>
{
  public CounterpartiesCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(100);
  }
}