namespace Argon.Application.Transactions.SetCounterparty;

[UsedImplicitly]
public class TransactionsSetCounterpartyValidator : AbstractValidator<TransactionsSetCounterpartyRequest>
{
  public TransactionsSetCounterpartyValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.CounterpartyId)
      .MustAsync(async (id, cancellationToken) => await dbContext.Counterparties.AnyAsync(counterparty => counterparty.Id == id, cancellationToken))
      .WithMessage("The counterparty id {PropertyValue} does not exist");
  }
}
