namespace Argon.Application.CounterpartyIdentifiers.Update;

[UsedImplicitly]
public class CounterpartyIdentifiersUpdateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartyIdentifiersUpdateRequest>
{
  public async Task Handle(CounterpartyIdentifiersUpdateRequest request, CancellationToken cancellationToken)
  {
    CounterpartyIdentifier? entity = await dbContext
      .CounterpartyIdentifiers
      .Where(counterpartyIdentifier => counterpartyIdentifier.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(CounterpartyIdentifier), request.Id);
    }

    entity.CounterpartyId = request.CounterpartyId;
    entity.IdentifierText = request.IdentifierText;

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
