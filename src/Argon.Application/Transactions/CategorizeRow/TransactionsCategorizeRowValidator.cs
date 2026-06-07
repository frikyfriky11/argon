namespace Argon.Application.Transactions.CategorizeRow;

[UsedImplicitly]
public class TransactionsCategorizeRowValidator : AbstractValidator<TransactionsCategorizeRowRequest>
{
  public TransactionsCategorizeRowValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.AccountId)
      .MustAsync(async (id, cancellationToken) => await dbContext.Accounts.AnyAsync(account => account.Id == id, cancellationToken))
      .WithMessage("The account id {PropertyValue} does not exist");
  }
}
