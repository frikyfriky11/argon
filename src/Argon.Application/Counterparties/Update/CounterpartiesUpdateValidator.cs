namespace Argon.Application.Counterparties.Update;

[UsedImplicitly]
public class CounterpartiesUpdateValidator : AbstractValidator<CounterpartiesUpdateRequest>
{
  public CounterpartiesUpdateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Name)
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(100)
      .MustAsync(async (request, name, cancellationToken) =>
        !await dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id != request.Id && counterparty.Name.ToLower() == name.ToLower(), cancellationToken))
      .WithMessage("A counterparty named '{PropertyValue}' already exists");
  }
}