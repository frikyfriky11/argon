namespace Argon.Application.Transactions;

/// <summary>
///   The row of a transaction create request
/// </summary>
/// <param name="Id">The id of the transaction row</param>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsUpdateRequest(Guid? Id, int RowCounter, Guid AccountId, decimal? Debit, decimal? Credit,
  string? Description);

/// <summary>
///   The request to update an existing transaction
/// </summary>
/// <param name="Date">The date of the transaction</param>
/// <param name="Description">The description of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsUpdateRequest(DateOnly Date, string Description,
  List<TransactionRowsUpdateRequest> TransactionRows) : IRequest
{
  /// <summary>
  ///   This field is used only internally to manually bind the [FromRoute] Guid id attribute.
  ///   It is not displayed in the documentation because the user of the API should use the route parameter.
  ///   This cannot be made internal because it would cause conflicts since you couldn't ever set it.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid Id { get; set; }
}

[UsedImplicitly]
public class TransactionsUpdateRequestValidator : AbstractValidator<TransactionsUpdateRequest>
{
  public TransactionsUpdateRequestValidator(IApplicationDbContext dbContext)
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
public class TransactionsUpdateRequestHandler : IRequestHandler<TransactionsUpdateRequest>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsUpdateRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task Handle(TransactionsUpdateRequest request, CancellationToken cancellationToken)
  {
    Transaction? entity = await _dbContext
      .Transactions
      .Include(transaction => transaction.TransactionRows)
      .Where(transaction => transaction.Id == request.Id)
      .FirstOrDefaultAsync(cancellationToken);

    if (entity is null)
    {
      throw new NotFoundException(nameof(Transaction), request.Id);
    }

    _mapper.Map(request, entity);

    List<TransactionRowsUpdateRequest> tempRequestRows = request.TransactionRows;

    foreach (TransactionRow entityRow in entity.TransactionRows)
    {
      TransactionRowsUpdateRequest? requestRow = tempRequestRows.FirstOrDefault(r => r.Id == entityRow.Id);

      if (requestRow is not null)
      {
        _mapper.Map(requestRow, entityRow);
        tempRequestRows.Remove(requestRow);
      }
      else
      {
        entity.TransactionRows.Remove(entityRow);
      }
    }

    foreach (TransactionRowsUpdateRequest requestRow in tempRequestRows)
    {
      entity.TransactionRows.Add(_mapper.Map<TransactionRow>(requestRow));
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
