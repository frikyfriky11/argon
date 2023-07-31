namespace Argon.Application.Transactions;

/// <summary>
///   The row of a transaction create request
/// </summary>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsCreateRequest(int RowCounter, Guid AccountId, decimal? Debit, decimal? Credit, string? Description);

/// <summary>
///   The request to create a new Transaction entity
/// </summary>
/// <param name="Date">The date of the transaction</param>
/// <param name="Description">The description of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsCreateRequest
  (DateOnly Date, string Description, List<TransactionRowsCreateRequest> TransactionRows) : IRequest<TransactionsCreateResponse>;

/// <summary>
///   The result of the creation of a new Transaction entity
/// </summary>
/// <param name="Id">The id of the newly created Transaction</param>
[PublicAPI]
public record TransactionsCreateResponse(Guid Id);

[UsedImplicitly]
public class TransactionsCreateRequestValidator : AbstractValidator<TransactionsCreateRequest>
{
  public TransactionsCreateRequestValidator(IApplicationDbContext dbContext)
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

[UsedImplicitly]
public class TransactionsCreateRequestHandler : IRequestHandler<TransactionsCreateRequest, TransactionsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsCreateRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<TransactionsCreateResponse> Handle(TransactionsCreateRequest request, CancellationToken cancellationToken)
  {
    Transaction entity = _mapper.Map<Transaction>(request);

    await _dbContext.Transactions.AddAsync(entity, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new TransactionsCreateResponse(entity.Id);
  }
}
