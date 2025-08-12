namespace Argon.Application.CounterpartyIdentifiers.Get;

[UsedImplicitly]
public class CounterpartyIdentifiersGetHandler(
  IApplicationDbContext dbContext
): IRequestHandler<CounterpartyIdentifiersGetRequest, CounterpartyIdentifiersGetResponse>
{
  public async Task<CounterpartyIdentifiersGetResponse> Handle(CounterpartyIdentifiersGetRequest request, CancellationToken cancellationToken)
  {
    CounterpartyIdentifiersGetResponse? result = await dbContext
      .CounterpartyIdentifiers
      .AsNoTracking()
      .Where(counterpartyIdentifier => counterpartyIdentifier.Id == request.Id)
      .Select(counterpartyIdentifier => new CounterpartyIdentifiersGetResponse(
        counterpartyIdentifier.Id,
        counterpartyIdentifier.CounterpartyId,
        counterpartyIdentifier.IdentifierText
      ))
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
