namespace Argon.Application.Counterparties.Get;

[UsedImplicitly]
public class CounterpartiesGetHandler(
  IApplicationDbContext dbContext
): IRequestHandler<CounterpartiesGetRequest, CounterpartiesGetResponse>
{
  public async Task<CounterpartiesGetResponse> Handle(CounterpartiesGetRequest request, CancellationToken cancellationToken)
  {
    CounterpartiesGetResponse? result = await dbContext
      .Counterparties
      .AsNoTracking()
      .Where(counterparty => counterparty.Id == request.Id)
      .Select(counterparty => new CounterpartiesGetResponse(
        counterparty.Id,
        counterparty.Name
      ))
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
