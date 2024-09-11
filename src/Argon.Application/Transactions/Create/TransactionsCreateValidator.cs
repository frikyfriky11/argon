namespace Argon.Application.Transactions.Create;

[UsedImplicitly]
public class TransactionsCreateValidator : AbstractValidator<TransactionsCreateRequest>
{
  public TransactionsCreateValidator(IApplicationDbContext dbContext)
  {
    RuleFor(request => request.Description)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(request => request.TransactionRows)
      .NotEmpty()
      .Must(rows => rows.Count >= 2).WithMessage("There should be at least two rows in a transaction")
      .Custom((rows, context) =>
      {
        decimal debitSum = rows.Sum(row => row.Debit ?? 0);
        decimal creditSum = rows.Sum(row => row.Credit ?? 0);

        decimal missing = debitSum - creditSum;

        switch (missing)
        {
          case < 0:
            context.AddFailure($"The sum of the transaction rows should be zero but {Math.Abs(missing)} are missing in debit amounts");
            break;
          case > 0:
            context.AddFailure($"The sum of the transaction rows should be zero but {Math.Abs(missing)} are missing in credit amounts");
            break;
        }
      });

    RuleForEach(request => request.TransactionRows).ChildRules(rows =>
    {
      rows.RuleFor(row => row.AccountId)
        .MustAsync(async (id, cancellationToken) => await dbContext.Accounts.AnyAsync(account => account.Id == id, cancellationToken))
        .WithMessage("The account id {PropertyValue} does not exist");

      rows.RuleFor(row => row)
        .Must(row => !(row.Credit is null or 0 && row.Debit is null or 0)).WithMessage("Credit and debit amounts cannot be empty at the same time on the same row");

      rows.RuleFor(row => row.Credit)
        .PrecisionScale(12, 2, true);

      rows.RuleFor(row => row.Debit)
        .PrecisionScale(12, 2, true);

      rows.RuleFor(row => row.Description)
        .MaximumLength(100);
    });
  }
}