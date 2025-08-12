namespace Argon.Application.CounterpartyIdentifiers.Update;

[UsedImplicitly]
public class CounterpartyIdentifiersUpdateValidator : AbstractValidator<CounterpartyIdentifiersUpdateRequest>
{
  public CounterpartyIdentifiersUpdateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.CounterpartyId)
      .MustAsync(async (id, cancellationToken) => await dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id == id, cancellationToken))
      .WithMessage("The counterparty id {PropertyValue} does not exist");

    RuleFor(request => request.IdentifierText)
      .NotEmpty()
      .MaximumLength(250);
  }
}