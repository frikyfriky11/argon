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
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(250)
      .MustAsync(async (request, text, cancellationToken) =>
        !await dbContext.CounterpartyIdentifiers.AnyAsync(identifier => identifier.Id != request.Id && identifier.IdentifierText.ToLower() == text.ToLower(), cancellationToken))
      .WithMessage("A counterparty identifier with text '{PropertyValue}' already exists");
  }
}