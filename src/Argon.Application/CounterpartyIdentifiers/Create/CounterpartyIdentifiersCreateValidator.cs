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
      .Cascade(CascadeMode.Stop)
      .NotEmpty()
      .MaximumLength(250)
      .MustAsync(async (text, cancellationToken) =>
        !await dbContext.CounterpartyIdentifiers.AnyAsync(identifier => identifier.IdentifierText.ToLower() == text.ToLower(), cancellationToken))
      .WithMessage("A counterparty identifier with text '{PropertyValue}' already exists");
  }
}