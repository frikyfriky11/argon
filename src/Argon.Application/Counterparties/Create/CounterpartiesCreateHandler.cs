namespace Argon.Application.Counterparties.Create;

[UsedImplicitly]
public class CounterpartiesCreateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartiesCreateRequest, CounterpartiesCreateResponse>
{
  public async Task<CounterpartiesCreateResponse> Handle(CounterpartiesCreateRequest request, CancellationToken cancellationToken)
  {
    Counterparty entity = new()
    {
      Name = request.Name,
    };

    await dbContext.Counterparties.AddAsync(entity, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new CounterpartiesCreateResponse(entity.Id);
  }
}