namespace Argon.Application.CounterpartyIdentifiers.Delete;

[UsedImplicitly]
public class CounterpartyIdentifiersDeleteHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartyIdentifiersDeleteRequest>
{
  public async Task Handle(CounterpartyIdentifiersDeleteRequest request, CancellationToken cancellationToken)
  {
    CounterpartyIdentifier? entity = await dbContext
      .CounterpartyIdentifiers
      .Where(counterpartyIdentifier => counterpartyIdentifier.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(CounterpartyIdentifier), request.Id);
    }

    dbContext.CounterpartyIdentifiers.Remove(entity);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
