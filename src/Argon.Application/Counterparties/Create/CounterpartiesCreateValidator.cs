namespace Argon.Application.Counterparties.Create;

[UsedImplicitly]
public class CounterpartiesCreateValidator : AbstractValidator<CounterpartiesCreateRequest>
{
  public CounterpartiesCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(100)
      .MustAsync(async (name, cancellationToken) =>
        !await dbContext.Counterparties.AnyAsync(counterparty => counterparty.Name.ToLower() == name.ToLower(), cancellationToken))
      .WithMessage("A counterparty named '{PropertyValue}' already exists");
  }
}