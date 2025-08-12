namespace Argon.Application.CounterpartyIdentifiers.Create;

[UsedImplicitly]
public class CounterpartyIdentifiersCreateValidator : AbstractValidator<CounterpartyIdentifiersCreateRequest>
{
  public CounterpartyIdentifiersCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.CounterpartyId)
      .MustAsync(async (id, cancellationToken) => await dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id == id, cancellationToken))
      .WithMessage("The counterparty id {PropertyValue} does not exist");

    RuleFor(request => request.IdentifierText)
      .NotEmpty()
      .MaximumLength(250);
  }
}