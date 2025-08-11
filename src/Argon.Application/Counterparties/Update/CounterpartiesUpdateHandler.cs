namespace Argon.Application.Counterparties.Update;

[UsedImplicitly]
public class CounterpartiesUpdateHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartiesUpdateRequest>
{
  public async Task Handle(CounterpartiesUpdateRequest request, CancellationToken cancellationToken)
  {
    Counterparty? entity = await dbContext
      .Counterparties
      .Where(counterparty => counterparty.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Counterparty), request.Id);
    }

    entity.Name = request.Name;

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
