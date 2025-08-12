namespace Argon.Application.CounterpartyIdentifiers.Create;

[UsedImplicitly]
public class CounterpartyIdentifiersCreateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartyIdentifiersCreateRequest, CounterpartyIdentifiersCreateResponse>
{
  public async Task<CounterpartyIdentifiersCreateResponse> Handle(CounterpartyIdentifiersCreateRequest request, CancellationToken cancellationToken)
  {
    CounterpartyIdentifier entity = new()
    {
      CounterpartyId = request.CounterpartyId,
      IdentifierText = request.IdentifierText,
    };

    await dbContext.CounterpartyIdentifiers.AddAsync(entity, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new CounterpartyIdentifiersCreateResponse(entity.Id);
  }
}