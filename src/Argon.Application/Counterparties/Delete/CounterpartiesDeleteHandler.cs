namespace Argon.Application.Counterparties.Delete;

[UsedImplicitly]
public class CounterpartiesDeleteHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartiesDeleteRequest>
{
  public async Task Handle(CounterpartiesDeleteRequest request, CancellationToken cancellationToken)
  {
    Counterparty? entity = await dbContext
      .Counterparties
      .Where(counterparty => counterparty.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Counterparty), request.Id);
    }

    dbContext.Counterparties.Remove(entity);

    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
