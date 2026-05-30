namespace Argon.Application.Counterparties.Create;

[UsedImplicitly]
public class CounterpartiesCreateValidator : AbstractValidator<CounterpartiesCreateRequest>
{
  public CounterpartiesCreateValidator()
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(100);
  }
}