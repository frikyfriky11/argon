namespace Argon.Application.Counterparties.Update;

[UsedImplicitly]
public class CounterpartiesUpdateValidator : AbstractValidator<CounterpartiesUpdateRequest>
{
  public CounterpartiesUpdateValidator()
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(100);
  }
}